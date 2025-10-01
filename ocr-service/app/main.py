from fastapi import FastAPI, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from loguru import logger
import sys
from datetime import datetime

from app.core.config import settings
from app.api.endpoints import router
from app.services.redis_service import get_redis_service
from app.services.ocr_service import get_ocr_engine
from app.models.schemas import HealthResponse


# Configure logging
logger.remove()
logger.add(
    sys.stdout,
    format="<green>{time:YYYY-MM-DD HH:mm:ss}</green> | <level>{level: <8}</level> | <cyan>{name}</cyan>:<cyan>{function}</cyan>:<cyan>{line}</cyan> - <level>{message}</level>",
    level=settings.LOG_LEVEL
)
logger.add(
    settings.LOG_FILE,
    rotation="10 MB",
    retention="10 days",
    level=settings.LOG_LEVEL
)


# Create FastAPI app
app = FastAPI(
    title=settings.APP_NAME,
    version=settings.APP_VERSION,
    description="OCR Microservice for Thai Bank Slip Verification",
    docs_url="/docs",
    redoc_url="/redoc"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify allowed origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# Include API router
app.include_router(router, prefix=settings.API_PREFIX, tags=["OCR"])


@app.on_event("startup")
async def startup_event():
    """Initialize services on startup"""
    logger.info(f"Starting {settings.APP_NAME} v{settings.APP_VERSION}")
    
    # Initialize Redis
    redis = get_redis_service()
    if redis.is_connected():
        logger.info("Redis connection established")
    else:
        logger.warning("Redis not available - caching disabled")
    
    # Initialize OCR engine
    ocr_engine = get_ocr_engine()
    if ocr_engine.is_available():
        logger.info("OCR engine initialized")
    else:
        logger.error("No OCR engine available!")


@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    logger.info(f"Shutting down {settings.APP_NAME}")


@app.get("/", response_model=dict)
async def root():
    """Root endpoint"""
    return {
        "service": settings.APP_NAME,
        "version": settings.APP_VERSION,
        "status": "running",
        "docs": "/docs",
        "api": settings.API_PREFIX
    }


@app.get("/health", response_model=HealthResponse)
async def health_check():
    """Health check endpoint"""
    redis = get_redis_service()
    redis_connected = redis.is_connected()
    
    return HealthResponse(
        status="healthy" if redis_connected else "degraded",
        version=settings.APP_VERSION,
        timestamp=datetime.utcnow(),
        redis_connected=redis_connected
    )


@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    """Global exception handler"""
    logger.error(f"Unhandled exception: {exc}", exc_info=True)
    return JSONResponse(
        status_code=500,
        content={
            "detail": "Internal server error",
            "error": str(exc) if settings.DEBUG else "An error occurred"
        }
    )


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        "main:app",
        host=settings.HOST,
        port=settings.PORT,
        reload=settings.DEBUG,
        log_level=settings.LOG_LEVEL.lower()
    )
