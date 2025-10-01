import numpy as np
from typing import Tuple, Optional, List, Dict
from loguru import logger
import time

try:
    from paddleocr import PaddleOCR
    PADDLE_AVAILABLE = True
except ImportError:
    PADDLE_AVAILABLE = False
    logger.warning("PaddleOCR not available")

try:
    import easyocr
    EASYOCR_AVAILABLE = True
except ImportError:
    EASYOCR_AVAILABLE = False
    logger.warning("EasyOCR not available")


class OCREngine:
    """OCR engine with multiple backends and fallback support"""
    
    def __init__(self, languages: List[str] = None, use_gpu: bool = False):
        """
        Initialize OCR engines
        
        Args:
            languages: List of language codes (e.g., ['th', 'en'])
            use_gpu: Whether to use GPU acceleration
        """
        self.languages = languages or ['th', 'en']
        self.use_gpu = use_gpu
        
        # Initialize engines
        self.paddle_ocr = None
        self.easy_ocr = None
        
        if PADDLE_AVAILABLE:
            try:
                self.paddle_ocr = PaddleOCR(
                    use_angle_cls=True,
                    lang='en',  # PaddleOCR doesn't have direct Thai support, but works with mixed text
                    use_gpu=use_gpu,
                    show_log=False
                )
                logger.info("PaddleOCR initialized successfully")
            except Exception as e:
                logger.error(f"Failed to initialize PaddleOCR: {e}")
        
        if EASYOCR_AVAILABLE:
            try:
                self.easy_ocr = easyocr.Reader(
                    self.languages,
                    gpu=use_gpu,
                    verbose=False
                )
                logger.info("EasyOCR initialized successfully")
            except Exception as e:
                logger.error(f"Failed to initialize EasyOCR: {e}")
    
    def process_with_paddle(self, image: np.ndarray) -> Tuple[str, float]:
        """
        Process image with PaddleOCR
        
        Args:
            image: Input image as numpy array
            
        Returns:
            Tuple of (text, confidence)
        """
        if not self.paddle_ocr:
            raise RuntimeError("PaddleOCR not available")
        
        try:
            result = self.paddle_ocr.ocr(image, cls=True)
            
            if not result or not result[0]:
                return "", 0.0
            
            texts = []
            confidences = []
            
            for line in result[0]:
                if len(line) >= 2:
                    text = line[1][0]
                    conf = line[1][1]
                    texts.append(text)
                    confidences.append(conf)
            
            full_text = "\n".join(texts)
            avg_confidence = sum(confidences) / len(confidences) if confidences else 0.0
            
            return full_text, avg_confidence
            
        except Exception as e:
            logger.error(f"PaddleOCR processing error: {e}")
            raise
    
    def process_with_easyocr(self, image: np.ndarray) -> Tuple[str, float]:
        """
        Process image with EasyOCR
        
        Args:
            image: Input image as numpy array
            
        Returns:
            Tuple of (text, confidence)
        """
        if not self.easy_ocr:
            raise RuntimeError("EasyOCR not available")
        
        try:
            result = self.easy_ocr.readtext(image)
            
            if not result:
                return "", 0.0
            
            texts = []
            confidences = []
            
            for detection in result:
                text = detection[1]
                conf = detection[2]
                texts.append(text)
                confidences.append(conf)
            
            full_text = "\n".join(texts)
            avg_confidence = sum(confidences) / len(confidences) if confidences else 0.0
            
            return full_text, avg_confidence
            
        except Exception as e:
            logger.error(f"EasyOCR processing error: {e}")
            raise
    
    def process(
        self,
        image: np.ndarray,
        engine: Optional[str] = None
    ) -> Dict[str, any]:
        """
        Process image with OCR with fallback support
        
        Args:
            image: Input image as numpy array
            engine: Specific engine to use ('paddleocr', 'easyocr', or None for auto)
            
        Returns:
            Dictionary with text, confidence, and engine used
        """
        start_time = time.time()
        
        # Try specified engine first
        if engine == "paddleocr" and self.paddle_ocr:
            try:
                text, confidence = self.process_with_paddle(image)
                processing_time = time.time() - start_time
                return {
                    "text": text,
                    "confidence": confidence,
                    "engine": "paddleocr",
                    "processing_time": processing_time
                }
            except Exception as e:
                logger.warning(f"PaddleOCR failed, trying fallback: {e}")
        
        if engine == "easyocr" and self.easy_ocr:
            try:
                text, confidence = self.process_with_easyocr(image)
                processing_time = time.time() - start_time
                return {
                    "text": text,
                    "confidence": confidence,
                    "engine": "easyocr",
                    "processing_time": processing_time
                }
            except Exception as e:
                logger.warning(f"EasyOCR failed: {e}")
        
        # Try all available engines with fallback
        engines_to_try = []
        if self.paddle_ocr:
            engines_to_try.append(("paddleocr", self.process_with_paddle))
        if self.easy_ocr:
            engines_to_try.append(("easyocr", self.process_with_easyocr))
        
        for engine_name, process_func in engines_to_try:
            try:
                text, confidence = process_func(image)
                processing_time = time.time() - start_time
                return {
                    "text": text,
                    "confidence": confidence,
                    "engine": engine_name,
                    "processing_time": processing_time
                }
            except Exception as e:
                logger.error(f"{engine_name} failed: {e}")
                continue
        
        # All engines failed
        processing_time = time.time() - start_time
        return {
            "text": "",
            "confidence": 0.0,
            "engine": "none",
            "processing_time": processing_time
        }
    
    def is_available(self) -> bool:
        """Check if at least one OCR engine is available"""
        return self.paddle_ocr is not None or self.easy_ocr is not None


# Global OCR engine instance
_ocr_engine: Optional[OCREngine] = None


def get_ocr_engine() -> OCREngine:
    """Get or create OCR engine instance"""
    global _ocr_engine
    if _ocr_engine is None:
        _ocr_engine = OCREngine()
    return _ocr_engine
