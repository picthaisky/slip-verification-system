"""
Health and monitoring endpoints for OCR service
"""
from fastapi import APIRouter
from typing import Dict, Any
from datetime import datetime
import sys
import platform

from app.services.ocr_service import get_ocr_engine, PADDLE_AVAILABLE, EASYOCR_AVAILABLE
from app.services.redis_service import get_redis_service
from app.core.config import settings

router = APIRouter(tags=["Health"])


@router.get("/health")
async def health_check() -> Dict[str, Any]:
    """
    Health check endpoint for container orchestration
    
    Returns service health status including:
    - Overall status (healthy/degraded/unhealthy)
    - OCR engine availability
    - Redis connection status
    - System information
    """
    health_status = "healthy"
    issues = []
    
    # Check OCR engine
    ocr_engine = get_ocr_engine()
    ocr_available = ocr_engine.is_available()
    if not ocr_available:
        health_status = "degraded"
        issues.append("OCR engine not available")
    
    # Check Redis
    redis = get_redis_service()
    redis_connected = redis.is_connected()
    if not redis_connected:
        if health_status == "healthy":
            health_status = "degraded"
        issues.append("Redis not connected")
    
    return {
        "status": health_status,
        "timestamp": datetime.utcnow().isoformat(),
        "version": settings.API_VERSION,
        "service": settings.API_TITLE,
        "components": {
            "ocr_engine": {
                "status": "up" if ocr_available else "down",
                "paddleocr": PADDLE_AVAILABLE,
                "easyocr": EASYOCR_AVAILABLE
            },
            "redis": {
                "status": "up" if redis_connected else "down",
                "host": settings.REDIS_HOST
            }
        },
        "issues": issues if issues else None
    }


@router.get("/health/live")
async def liveness_probe() -> Dict[str, str]:
    """
    Kubernetes liveness probe endpoint
    
    Returns 200 if the service is running
    """
    return {"status": "alive"}


@router.get("/health/ready")
async def readiness_probe() -> Dict[str, Any]:
    """
    Kubernetes readiness probe endpoint
    
    Returns 200 only if the service is ready to accept requests
    """
    ocr_engine = get_ocr_engine()
    is_ready = ocr_engine.is_available()
    
    return {
        "status": "ready" if is_ready else "not_ready",
        "ocr_available": is_ready
    }


@router.get("/metrics")
async def get_metrics() -> Dict[str, Any]:
    """
    Basic metrics endpoint for monitoring
    
    Returns service metrics including:
    - System information
    - Configuration
    - Engine status
    """
    ocr_engine = get_ocr_engine()
    
    return {
        "timestamp": datetime.utcnow().isoformat(),
        "service": {
            "name": settings.API_TITLE,
            "version": settings.API_VERSION,
            "environment": settings.ENVIRONMENT
        },
        "system": {
            "python_version": sys.version,
            "platform": platform.platform(),
            "processor": platform.processor()
        },
        "configuration": {
            "max_image_size_mb": settings.MAX_IMAGE_SIZE / (1024 * 1024),
            "batch_size": settings.BATCH_SIZE,
            "allowed_extensions": list(settings.ALLOWED_EXTENSIONS),
            "rate_limit_enabled": settings.RATE_LIMIT_ENABLED,
            "api_key_enabled": settings.API_KEY_ENABLED
        },
        "engines": {
            "paddleocr": PADDLE_AVAILABLE,
            "easyocr": EASYOCR_AVAILABLE,
            "current": ocr_engine.engine if ocr_engine.is_available() else None
        }
    }


@router.get("/info")
async def service_info() -> Dict[str, Any]:
    """
    Service information endpoint
    
    Returns API documentation and configuration info
    """
    return {
        "name": settings.API_TITLE,
        "description": settings.API_DESCRIPTION,
        "version": settings.API_VERSION,
        "documentation": {
            "openapi": "/docs",
            "redoc": "/redoc"
        },
        "supported_formats": list(settings.ALLOWED_EXTENSIONS),
        "supported_banks": [
            "Bangkok Bank (BBL)",
            "Kasikorn Bank (KBANK)",
            "Siam Commercial Bank (SCB)",
            "Krungthai Bank (KTB)",
            "Bank of Ayudhya/Krungsri (BAY)",
            "TMBThanachart Bank (TTB)",
            "Government Savings Bank (GSB)",
            "Bank for Agriculture (BAAC)",
            "UOB Thailand",
            "CIMB Thai"
        ],
        "endpoints": {
            "process": "POST /api/v1/process - Process single image",
            "batch": "POST /api/v1/batch - Process multiple images",
            "status": "GET /api/v1/status/{job_id} - Get job status",
            "result": "GET /api/v1/result/{job_id} - Get OCR result"
        }
    }
