import redis
import json
from typing import Optional, Any
from loguru import logger
from app.core.config import settings


class RedisService:
    """Redis service for caching and queue management"""
    
    def __init__(self):
        """Initialize Redis connection"""
        self.client: Optional[redis.Redis] = None
        self._connect()
    
    def _connect(self):
        """Connect to Redis"""
        try:
            self.client = redis.Redis(
                host=settings.REDIS_HOST,
                port=settings.REDIS_PORT,
                db=settings.REDIS_DB,
                password=settings.REDIS_PASSWORD,
                decode_responses=True,
                socket_connect_timeout=5
            )
            # Test connection
            self.client.ping()
            logger.info("Redis connected successfully")
        except Exception as e:
            logger.error(f"Redis connection failed: {e}")
            self.client = None
    
    def is_connected(self) -> bool:
        """Check if Redis is connected"""
        if not self.client:
            return False
        try:
            self.client.ping()
            return True
        except:
            return False
    
    def set(self, key: str, value: Any, ttl: Optional[int] = None) -> bool:
        """
        Set a value in Redis
        
        Args:
            key: Cache key
            value: Value to cache (will be JSON serialized)
            ttl: Time to live in seconds
            
        Returns:
            True if successful, False otherwise
        """
        if not self.client:
            return False
        
        try:
            serialized = json.dumps(value)
            if ttl:
                self.client.setex(key, ttl, serialized)
            else:
                self.client.set(key, serialized)
            return True
        except Exception as e:
            logger.error(f"Redis set error: {e}")
            return False
    
    def get(self, key: str) -> Optional[Any]:
        """
        Get a value from Redis
        
        Args:
            key: Cache key
            
        Returns:
            Cached value or None
        """
        if not self.client:
            return None
        
        try:
            value = self.client.get(key)
            if value:
                return json.loads(value)
            return None
        except Exception as e:
            logger.error(f"Redis get error: {e}")
            return None
    
    def delete(self, key: str) -> bool:
        """
        Delete a key from Redis
        
        Args:
            key: Cache key
            
        Returns:
            True if successful, False otherwise
        """
        if not self.client:
            return False
        
        try:
            self.client.delete(key)
            return True
        except Exception as e:
            logger.error(f"Redis delete error: {e}")
            return False
    
    def exists(self, key: str) -> bool:
        """
        Check if a key exists in Redis
        
        Args:
            key: Cache key
            
        Returns:
            True if exists, False otherwise
        """
        if not self.client:
            return False
        
        try:
            return self.client.exists(key) > 0
        except Exception as e:
            logger.error(f"Redis exists error: {e}")
            return False
    
    def set_hash(self, key: str, field: str, value: Any) -> bool:
        """
        Set a hash field in Redis
        
        Args:
            key: Hash key
            field: Field name
            value: Field value
            
        Returns:
            True if successful, False otherwise
        """
        if not self.client:
            return False
        
        try:
            serialized = json.dumps(value)
            self.client.hset(key, field, serialized)
            return True
        except Exception as e:
            logger.error(f"Redis hset error: {e}")
            return False
    
    def get_hash(self, key: str, field: str) -> Optional[Any]:
        """
        Get a hash field from Redis
        
        Args:
            key: Hash key
            field: Field name
            
        Returns:
            Field value or None
        """
        if not self.client:
            return None
        
        try:
            value = self.client.hget(key, field)
            if value:
                return json.loads(value)
            return None
        except Exception as e:
            logger.error(f"Redis hget error: {e}")
            return None
    
    def get_all_hash(self, key: str) -> Optional[dict]:
        """
        Get all fields from a hash in Redis
        
        Args:
            key: Hash key
            
        Returns:
            Dictionary of all fields or None
        """
        if not self.client:
            return None
        
        try:
            data = self.client.hgetall(key)
            if data:
                return {k: json.loads(v) for k, v in data.items()}
            return None
        except Exception as e:
            logger.error(f"Redis hgetall error: {e}")
            return None


# Global Redis service instance
_redis_service: Optional[RedisService] = None


def get_redis_service() -> RedisService:
    """Get or create Redis service instance"""
    global _redis_service
    if _redis_service is None:
        _redis_service = RedisService()
    return _redis_service
