import uuid
from typing import Optional
from datetime import datetime
from loguru import logger
import time

from app.models.schemas import (
    OcrResult,
    ProcessingStatus,
    ExtractedData,
    BankInfo
)
from app.services.ocr_service import get_ocr_engine
from app.services.redis_service import get_redis_service
from app.utils.image_preprocessing import ImagePreprocessor
from app.utils.data_extraction import DataExtractor
from app.core.config import settings


class ProcessingService:
    """Main processing service for OCR and data extraction"""
    
    def __init__(self):
        self.ocr_engine = get_ocr_engine()
        self.redis = get_redis_service()
        self.preprocessor = ImagePreprocessor()
    
    def generate_job_id(self) -> str:
        """Generate unique job ID"""
        return str(uuid.uuid4())
    
    async def process_image(
        self,
        image_data: bytes,
        job_id: Optional[str] = None,
        preprocess: bool = True,
        ocr_engine: Optional[str] = None
    ) -> OcrResult:
        """
        Process image with OCR and data extraction
        
        Args:
            image_data: Image bytes
            job_id: Job ID (generated if not provided)
            preprocess: Whether to preprocess image
            ocr_engine: Specific OCR engine to use
            
        Returns:
            OcrResult object
        """
        if not job_id:
            job_id = self.generate_job_id()
        
        start_time = time.time()
        
        # Create initial result
        result = OcrResult(
            job_id=job_id,
            status=ProcessingStatus.PROCESSING,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        
        # Save initial status to Redis
        self._save_result(result)
        
        try:
            # Preprocess image
            if preprocess:
                logger.info(f"Preprocessing image for job {job_id}")
                image = self.preprocessor.preprocess_image(image_data)
            else:
                image = self.preprocessor._to_numpy(image_data)
            
            # Perform OCR
            logger.info(f"Performing OCR for job {job_id}")
            ocr_result = self.ocr_engine.process(image, engine=ocr_engine)
            
            raw_text = ocr_result["text"]
            confidence = ocr_result["confidence"]
            engine_used = ocr_result["engine"]
            
            if not raw_text:
                raise ValueError("No text extracted from image")
            
            # Extract structured data
            logger.info(f"Extracting data for job {job_id}")
            extracted = DataExtractor.extract_all(raw_text)
            
            # Create extracted data model
            bank = None
            if extracted.get("bank"):
                bank = BankInfo(**extracted["bank"])
            
            extracted_data = ExtractedData(
                amount=extracted.get("amount"),
                transaction_date=extracted.get("transaction_date"),
                transaction_time=extracted.get("transaction_time"),
                reference_number=extracted.get("reference_number"),
                bank=bank,
                sender_account=extracted.get("sender_account"),
                receiver_account=extracted.get("receiver_account"),
                sender_name=extracted.get("sender_name"),
                receiver_name=extracted.get("receiver_name")
            )
            
            # Update result
            processing_time = time.time() - start_time
            result.status = ProcessingStatus.COMPLETED
            result.raw_text = raw_text
            result.extracted_data = extracted_data
            result.confidence = confidence
            result.ocr_engine = engine_used
            result.processing_time = processing_time
            result.updated_at = datetime.utcnow()
            
            logger.info(f"Job {job_id} completed successfully in {processing_time:.2f}s")
            
        except Exception as e:
            logger.error(f"Job {job_id} failed: {e}")
            processing_time = time.time() - start_time
            result.status = ProcessingStatus.FAILED
            result.error_message = str(e)
            result.processing_time = processing_time
            result.updated_at = datetime.utcnow()
        
        # Save final result to Redis
        self._save_result(result)
        
        return result
    
    def get_result(self, job_id: str) -> Optional[OcrResult]:
        """
        Get processing result from Redis
        
        Args:
            job_id: Job ID
            
        Returns:
            OcrResult object or None
        """
        cache_key = f"ocr:result:{job_id}"
        data = self.redis.get(cache_key)
        
        if data:
            return OcrResult(**data)
        return None
    
    def _save_result(self, result: OcrResult):
        """Save result to Redis with TTL"""
        cache_key = f"ocr:result:{result.job_id}"
        self.redis.set(
            cache_key,
            result.model_dump(),
            ttl=settings.REDIS_CACHE_TTL
        )
    
    async def process_batch(
        self,
        images: list[bytes],
        batch_id: str,
        preprocess: bool = True,
        ocr_engine: Optional[str] = None
    ) -> list[OcrResult]:
        """
        Process multiple images
        
        Args:
            images: List of image bytes
            batch_id: Batch ID
            preprocess: Whether to preprocess images
            ocr_engine: Specific OCR engine to use
            
        Returns:
            List of OcrResult objects
        """
        results = []
        
        for idx, image_data in enumerate(images):
            job_id = f"{batch_id}_{idx}"
            logger.info(f"Processing image {idx+1}/{len(images)} in batch {batch_id}")
            
            try:
                result = await self.process_image(
                    image_data=image_data,
                    job_id=job_id,
                    preprocess=preprocess,
                    ocr_engine=ocr_engine
                )
                results.append(result)
            except Exception as e:
                logger.error(f"Failed to process image {idx+1} in batch {batch_id}: {e}")
                # Create failed result
                result = OcrResult(
                    job_id=job_id,
                    status=ProcessingStatus.FAILED,
                    error_message=str(e),
                    created_at=datetime.utcnow(),
                    updated_at=datetime.utcnow()
                )
                results.append(result)
        
        return results


# Global processing service instance
_processing_service: Optional[ProcessingService] = None


def get_processing_service() -> ProcessingService:
    """Get or create processing service instance"""
    global _processing_service
    if _processing_service is None:
        _processing_service = ProcessingService()
    return _processing_service
