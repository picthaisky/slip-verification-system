"""
Pydantic schemas for OCR Service
"""
from datetime import datetime
from enum import Enum
from typing import Optional, List
from pydantic import BaseModel, Field


class ProcessingStatus(str, Enum):
    """Status of OCR processing job"""
    PENDING = "pending"
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"


class BankInfo(BaseModel):
    """Bank information extracted from slip"""
    name: str = Field(..., description="Bank name in English")
    code: str = Field(..., description="Bank code (e.g., KBANK, SCB)")
    
    class Config:
        json_schema_extra = {
            "example": {
                "name": "Kasikorn Bank",
                "code": "KBANK"
            }
        }


class ExtractedData(BaseModel):
    """Structured data extracted from bank slip"""
    amount: Optional[float] = Field(None, description="Transaction amount in THB")
    transaction_date: Optional[str] = Field(None, description="Transaction date")
    transaction_time: Optional[str] = Field(None, description="Transaction time")
    reference_number: Optional[str] = Field(None, description="Transaction reference number")
    bank: Optional[BankInfo] = Field(None, description="Bank information")
    sender_account: Optional[str] = Field(None, description="Sender account number")
    receiver_account: Optional[str] = Field(None, description="Receiver account number")
    sender_name: Optional[str] = Field(None, description="Sender name")
    receiver_name: Optional[str] = Field(None, description="Receiver name")
    
    class Config:
        json_schema_extra = {
            "example": {
                "amount": 1500.00,
                "transaction_date": "01/10/2024",
                "transaction_time": "14:30:45",
                "reference_number": "REF123456789",
                "bank": {
                    "name": "Kasikorn Bank",
                    "code": "KBANK"
                },
                "sender_account": "123-4-56789-0",
                "receiver_account": "987-6-54321-0",
                "sender_name": None,
                "receiver_name": None
            }
        }


class OcrResult(BaseModel):
    """Complete OCR result with extracted data"""
    job_id: str = Field(..., description="Unique job identifier")
    status: ProcessingStatus = Field(..., description="Processing status")
    raw_text: Optional[str] = Field(None, description="Raw OCR text output")
    extracted_data: Optional[ExtractedData] = Field(None, description="Structured extracted data")
    confidence: Optional[float] = Field(None, ge=0.0, le=1.0, description="OCR confidence score (0-1)")
    ocr_engine: Optional[str] = Field(None, description="OCR engine used (paddleocr, easyocr)")
    processing_time: Optional[float] = Field(None, description="Processing time in seconds")
    error_message: Optional[str] = Field(None, description="Error message if failed")
    created_at: datetime = Field(..., description="Job creation timestamp")
    updated_at: datetime = Field(..., description="Job last update timestamp")
    
    class Config:
        json_schema_extra = {
            "example": {
                "job_id": "550e8400-e29b-41d4-a716-446655440000",
                "status": "completed",
                "raw_text": "ธนาคารกสิกรไทย...",
                "extracted_data": {
                    "amount": 1500.00,
                    "transaction_date": "01/10/2024",
                    "transaction_time": "14:30:45",
                    "reference_number": "REF123456789",
                    "bank": {
                        "name": "Kasikorn Bank",
                        "code": "KBANK"
                    }
                },
                "confidence": 0.95,
                "ocr_engine": "paddleocr",
                "processing_time": 2.35,
                "error_message": None,
                "created_at": "2024-10-01T14:30:00Z",
                "updated_at": "2024-10-01T14:30:02Z"
            }
        }


class ProcessResponse(BaseModel):
    """Response for /process endpoint"""
    job_id: str = Field(..., description="Unique job identifier for tracking")
    status: ProcessingStatus = Field(..., description="Current processing status")
    message: str = Field(..., description="Status message")
    
    class Config:
        json_schema_extra = {
            "example": {
                "job_id": "550e8400-e29b-41d4-a716-446655440000",
                "status": "completed",
                "message": "Processing completed"
            }
        }


class StatusResponse(BaseModel):
    """Response for /status endpoint"""
    job_id: str = Field(..., description="Job identifier")
    status: ProcessingStatus = Field(..., description="Current processing status")
    progress: Optional[float] = Field(None, ge=0.0, le=100.0, description="Processing progress (0-100)")
    message: str = Field(..., description="Status message")
    
    class Config:
        json_schema_extra = {
            "example": {
                "job_id": "550e8400-e29b-41d4-a716-446655440000",
                "status": "processing",
                "progress": 50.0,
                "message": "Processing in progress"
            }
        }


class BatchProcessResponse(BaseModel):
    """Response for /batch endpoint"""
    batch_id: str = Field(..., description="Unique batch identifier")
    job_ids: List[str] = Field(..., description="List of individual job IDs")
    total: int = Field(..., description="Total number of images processed")
    message: str = Field(..., description="Batch status message")
    
    class Config:
        json_schema_extra = {
            "example": {
                "batch_id": "batch-550e8400-e29b-41d4-a716-446655440000",
                "job_ids": [
                    "batch-550e8400-e29b-41d4-a716-446655440000_0",
                    "batch-550e8400-e29b-41d4-a716-446655440000_1"
                ],
                "total": 2,
                "message": "Batch processing completed. 2 images processed."
            }
        }


class HealthResponse(BaseModel):
    """Response for /health endpoint"""
    status: str = Field(..., description="Service health status (healthy, degraded)")
    version: str = Field(..., description="Service version")
    timestamp: datetime = Field(..., description="Health check timestamp")
    redis_connected: bool = Field(..., description="Redis connection status")
    
    class Config:
        json_schema_extra = {
            "example": {
                "status": "healthy",
                "version": "1.0.0",
                "timestamp": "2024-10-01T14:30:00Z",
                "redis_connected": True
            }
        }
