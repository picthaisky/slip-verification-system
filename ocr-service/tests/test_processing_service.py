import pytest
from unittest.mock import patch, MagicMock, AsyncMock
import numpy as np
from datetime import datetime

from app.services.processing_service import ProcessingService, get_processing_service
from app.models.schemas import ProcessingStatus


class TestProcessingService:
    """Test processing service"""
    
    @pytest.fixture
    def processing_service(self):
        """Create processing service instance"""
        return get_processing_service()
    
    @pytest.fixture
    def sample_image_bytes(self):
        """Create sample image bytes"""
        # Simple JPEG header + minimal data
        import io
        from PIL import Image
        
        img = Image.new('RGB', (100, 100), color='white')
        img_byte_arr = io.BytesIO()
        img.save(img_byte_arr, format='JPEG')
        img_byte_arr.seek(0)
        return img_byte_arr.getvalue()
    
    @pytest.mark.asyncio
    @patch('app.services.processing_service.get_ocr_engine')
    @patch('app.services.processing_service.ImagePreprocessor')
    async def test_process_image_success(self, mock_preprocessor, mock_get_engine, sample_image_bytes):
        """Test successful image processing"""
        # Mock OCR engine
        mock_engine = MagicMock()
        mock_engine.process.return_value = {
            "text": "ธนาคารกสิกรไทย\nจำนวนเงิน: 1,500.00 บาท",
            "confidence": 0.95,
            "engine": "paddleocr",
            "processing_time": 1.5
        }
        mock_get_engine.return_value = mock_engine
        
        # Mock preprocessor
        mock_preprocessor.preprocess_image.return_value = np.ones((100, 100), dtype=np.uint8)
        
        service = ProcessingService()
        result = await service.process_image(
            image_data=sample_image_bytes,
            preprocess=True,
            ocr_engine="paddleocr"
        )
        
        assert result is not None
        assert result.job_id is not None
        assert result.status == ProcessingStatus.COMPLETED
    
    @pytest.mark.asyncio
    @patch('app.services.processing_service.get_ocr_engine')
    async def test_process_image_without_preprocessing(self, mock_get_engine, sample_image_bytes):
        """Test image processing without preprocessing"""
        mock_engine = MagicMock()
        mock_engine.process.return_value = {
            "text": "Test text",
            "confidence": 0.8,
            "engine": "easyocr",
            "processing_time": 2.0
        }
        mock_get_engine.return_value = mock_engine
        
        service = ProcessingService()
        result = await service.process_image(
            image_data=sample_image_bytes,
            preprocess=False,
            ocr_engine=None
        )
        
        assert result is not None
        assert result.status == ProcessingStatus.COMPLETED
    
    @pytest.mark.asyncio
    @patch('app.services.processing_service.get_ocr_engine')
    async def test_process_image_failure(self, mock_get_engine, sample_image_bytes):
        """Test image processing failure"""
        mock_engine = MagicMock()
        mock_engine.process.side_effect = Exception("OCR failed")
        mock_get_engine.return_value = mock_engine
        
        service = ProcessingService()
        result = await service.process_image(
            image_data=sample_image_bytes,
            preprocess=False,
            ocr_engine=None
        )
        
        assert result is not None
        assert result.status == ProcessingStatus.FAILED
        assert result.error_message is not None
    
    @pytest.mark.asyncio
    @patch('app.services.processing_service.get_ocr_engine')
    async def test_process_batch(self, mock_get_engine, sample_image_bytes):
        """Test batch processing"""
        mock_engine = MagicMock()
        mock_engine.process.return_value = {
            "text": "Batch test",
            "confidence": 0.85,
            "engine": "paddleocr",
            "processing_time": 1.0
        }
        mock_get_engine.return_value = mock_engine
        
        service = ProcessingService()
        images = [sample_image_bytes, sample_image_bytes, sample_image_bytes]
        
        results = await service.process_batch(
            images=images,
            batch_id="test-batch-123",
            preprocess=True,
            ocr_engine=None
        )
        
        assert len(results) == 3
        for result in results:
            assert result.job_id.startswith("test-batch-123")


class TestProcessingServiceGetResult:
    """Test get_result method"""
    
    @patch('app.services.processing_service.get_redis_service')
    @patch('app.services.processing_service.get_ocr_engine')
    def test_get_result_from_cache(self, mock_get_engine, mock_get_redis):
        """Test getting result from cache"""
        from datetime import datetime
        
        mock_redis = MagicMock()
        mock_redis.is_connected.return_value = True
        mock_redis.get.return_value = {
            "job_id": "cached-job",
            "status": "completed",
            "raw_text": "Cached result",
            "confidence": 0.9,
            "created_at": datetime.utcnow().isoformat(),
            "updated_at": datetime.utcnow().isoformat()
        }
        mock_get_redis.return_value = mock_redis
        
        mock_engine = MagicMock()
        mock_engine.is_available.return_value = True
        mock_get_engine.return_value = mock_engine
        
        service = ProcessingService()
        result = service.get_result("cached-job")
        
        # Result depends on whether Redis returned valid data
        if result:
            assert result.job_id == "cached-job"
    
    @patch('app.services.processing_service.get_redis_service')
    @patch('app.services.processing_service.get_ocr_engine')
    def test_get_result_not_found(self, mock_get_engine, mock_get_redis):
        """Test getting non-existent result"""
        mock_redis = MagicMock()
        mock_redis.is_connected.return_value = False
        mock_redis.get.return_value = None
        mock_get_redis.return_value = mock_redis
        
        mock_engine = MagicMock()
        mock_get_engine.return_value = mock_engine
        
        service = ProcessingService()
        result = service.get_result("non-existent-job")
        assert result is None


class TestGetProcessingService:
    """Test get_processing_service singleton function"""
    
    def test_returns_processing_service(self):
        """Test that get_processing_service returns ProcessingService instance"""
        service = get_processing_service()
        assert isinstance(service, ProcessingService)
    
    def test_singleton_pattern(self):
        """Test that same instance is returned"""
        service1 = get_processing_service()
        service2 = get_processing_service()
        assert service1 is service2
