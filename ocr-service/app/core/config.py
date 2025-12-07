from pydantic_settings import BaseSettings
from typing import Optional


class Settings(BaseSettings):
    """Application settings"""
    
    # App settings
    APP_NAME: str = "OCR Microservice"
    APP_VERSION: str = "1.0.0"
    DEBUG: bool = False
    
    # API settings
    API_PREFIX: str = "/api/ocr"
    HOST: str = "0.0.0.0"
    PORT: int = 8000
    
    # Redis settings
    REDIS_HOST: str = "redis"
    REDIS_PORT: int = 6379
    REDIS_DB: int = 0
    REDIS_PASSWORD: Optional[str] = None
    REDIS_CACHE_TTL: int = 3600  # 1 hour
    
    # OCR settings
    OCR_ENGINES: list[str] = ["paddleocr", "easyocr"]
    OCR_LANGUAGES: list[str] = ["th", "en"]
    OCR_CONFIDENCE_THRESHOLD: float = 0.6
    
    # Image processing settings
    MAX_IMAGE_SIZE: int = 10 * 1024 * 1024  # 10MB
    ALLOWED_EXTENSIONS: list[str] = ["jpg", "jpeg", "png"]
    IMAGE_PREPROCESSING: bool = True
    
    # Processing settings
    MAX_PROCESSING_TIME: int = 30  # seconds
    BATCH_SIZE: int = 10
    RETRY_ATTEMPTS: int = 3
    
    # Rate limiting settings
    RATE_LIMIT_ENABLED: bool = True
    RATE_LIMIT_REQUESTS: int = 100  # requests per window
    RATE_LIMIT_WINDOW: int = 60  # window in seconds
    
    # API Key settings (optional)
    API_KEY_ENABLED: bool = False
    API_KEY: Optional[str] = None
    
    # Logging settings
    LOG_LEVEL: str = "INFO"
    LOG_FILE: str = "logs/ocr_service.log"
    
    class Config:
        env_file = ".env"
        case_sensitive = True


settings = Settings()
