from fastapi import APIRouter, UploadFile, File, Form, HTTPException, status
from typing import Optional, List
from loguru import logger
import uuid

from app.models.schemas import (
    ProcessResponse,
    StatusResponse,
    OcrResult,
    ProcessingStatus,
    BatchProcessResponse
)
from app.services.processing_service import get_processing_service
from app.core.config import settings


router = APIRouter()


@router.post("/process", response_model=ProcessResponse)
async def process_image(
    file: UploadFile = File(..., description="Image file (JPG, PNG)"),
    preprocess: bool = Form(True, description="Enable image preprocessing"),
    ocr_engine: Optional[str] = Form(None, description="Specific OCR engine to use (paddleocr, easyocr)")
):
    """
    Process a single image and extract slip data
    
    - **file**: Image file to process
    - **preprocess**: Enable/disable image preprocessing
    - **ocr_engine**: Optional specific OCR engine to use
    
    Returns job_id for tracking the processing status
    """
    # Validate file type
    if not file.filename:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="No filename provided"
        )
    
    file_ext = file.filename.split(".")[-1].lower()
    if file_ext not in settings.ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Invalid file type. Allowed: {', '.join(settings.ALLOWED_EXTENSIONS)}"
        )
    
    # Read file data
    try:
        image_data = await file.read()
        
        # Validate file size
        if len(image_data) > settings.MAX_IMAGE_SIZE:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"File too large. Max size: {settings.MAX_IMAGE_SIZE / (1024*1024):.1f}MB"
            )
        
        if len(image_data) == 0:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Empty file"
            )
    except Exception as e:
        logger.error(f"Error reading file: {e}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Failed to read file"
        )
    
    # Process image
    try:
        processing_service = get_processing_service()
        result = await processing_service.process_image(
            image_data=image_data,
            preprocess=preprocess,
            ocr_engine=ocr_engine
        )
        
        return ProcessResponse(
            job_id=result.job_id,
            status=result.status,
            message="Processing completed" if result.status == ProcessingStatus.COMPLETED else "Processing failed"
        )
    except Exception as e:
        logger.error(f"Error processing image: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Processing failed: {str(e)}"
        )


@router.get("/status/{job_id}", response_model=StatusResponse)
async def get_status(job_id: str):
    """
    Get processing status for a job
    
    - **job_id**: Job ID returned from /process endpoint
    
    Returns current processing status
    """
    processing_service = get_processing_service()
    result = processing_service.get_result(job_id)
    
    if not result:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Job {job_id} not found"
        )
    
    # Calculate progress
    progress = None
    if result.status == ProcessingStatus.PENDING:
        progress = 0.0
    elif result.status == ProcessingStatus.PROCESSING:
        progress = 50.0
    elif result.status in [ProcessingStatus.COMPLETED, ProcessingStatus.FAILED]:
        progress = 100.0
    
    message = "Processing pending"
    if result.status == ProcessingStatus.PROCESSING:
        message = "Processing in progress"
    elif result.status == ProcessingStatus.COMPLETED:
        message = "Processing completed successfully"
    elif result.status == ProcessingStatus.FAILED:
        message = f"Processing failed: {result.error_message}"
    
    return StatusResponse(
        job_id=job_id,
        status=result.status,
        progress=progress,
        message=message
    )


@router.get("/result/{job_id}", response_model=OcrResult)
async def get_result(job_id: str):
    """
    Get OCR result for a completed job
    
    - **job_id**: Job ID returned from /process endpoint
    
    Returns complete OCR result with extracted data
    """
    processing_service = get_processing_service()
    result = processing_service.get_result(job_id)
    
    if not result:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=f"Job {job_id} not found"
        )
    
    return result


@router.post("/batch", response_model=BatchProcessResponse)
async def process_batch(
    files: List[UploadFile] = File(..., description="Multiple image files"),
    preprocess: bool = Form(True, description="Enable image preprocessing"),
    ocr_engine: Optional[str] = Form(None, description="Specific OCR engine to use")
):
    """
    Process multiple images in batch
    
    - **files**: List of image files to process
    - **preprocess**: Enable/disable image preprocessing
    - **ocr_engine**: Optional specific OCR engine to use
    
    Returns batch_id and individual job_ids for tracking
    """
    if len(files) > settings.BATCH_SIZE:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Too many files. Maximum batch size: {settings.BATCH_SIZE}"
        )
    
    if len(files) == 0:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="No files provided"
        )
    
    # Validate all files
    images_data = []
    for file in files:
        if not file.filename:
            continue
        
        file_ext = file.filename.split(".")[-1].lower()
        if file_ext not in settings.ALLOWED_EXTENSIONS:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Invalid file type for {file.filename}. Allowed: {', '.join(settings.ALLOWED_EXTENSIONS)}"
            )
        
        try:
            image_data = await file.read()
            if len(image_data) > settings.MAX_IMAGE_SIZE:
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=f"File {file.filename} too large"
                )
            images_data.append(image_data)
        except Exception as e:
            logger.error(f"Error reading file {file.filename}: {e}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Failed to read file {file.filename}"
            )
    
    # Process batch
    batch_id = str(uuid.uuid4())
    try:
        processing_service = get_processing_service()
        results = await processing_service.process_batch(
            images=images_data,
            batch_id=batch_id,
            preprocess=preprocess,
            ocr_engine=ocr_engine
        )
        
        job_ids = [result.job_id for result in results]
        
        return BatchProcessResponse(
            batch_id=batch_id,
            job_ids=job_ids,
            total=len(job_ids),
            message=f"Batch processing completed. {len(job_ids)} images processed."
        )
    except Exception as e:
        logger.error(f"Error processing batch: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Batch processing failed: {str(e)}"
        )
