import pytest
import numpy as np
from unittest.mock import Mock, patch, MagicMock
from app.services.ocr_service import OCREngine, get_ocr_engine, PADDLE_AVAILABLE, EASYOCR_AVAILABLE


class TestOCREngine:
    """Test OCR Engine class"""
    
    @pytest.fixture
    def sample_image(self):
        """Create a sample test image"""
        return np.ones((100, 100, 3), dtype=np.uint8) * 255
    
    @pytest.fixture
    def ocr_engine(self):
        """Create OCR engine instance for testing"""
        return OCREngine(languages=['en'], use_gpu=False)
    
    def test_init_default_languages(self):
        """Test default language initialization"""
        engine = OCREngine()
        assert 'th' in engine.languages
        assert 'en' in engine.languages
    
    def test_init_custom_languages(self):
        """Test custom language initialization"""
        engine = OCREngine(languages=['en'])
        assert engine.languages == ['en']
    
    def test_is_available(self, ocr_engine):
        """Test availability check"""
        # Should return True if at least one engine is available
        result = ocr_engine.is_available()
        assert isinstance(result, bool)
        # If both engines are unavailable, should be False
        if not PADDLE_AVAILABLE and not EASYOCR_AVAILABLE:
            assert result is False
    
    def test_process_returns_dict(self, ocr_engine, sample_image):
        """Test that process returns correct structure"""
        result = ocr_engine.process(sample_image)
        
        assert isinstance(result, dict)
        assert "text" in result
        assert "confidence" in result
        assert "engine" in result
        assert "processing_time" in result
    
    def test_process_with_no_engine(self, sample_image):
        """Test processing when no engine is available"""
        with patch.object(OCREngine, '__init__', lambda x, **kwargs: None):
            engine = OCREngine.__new__(OCREngine)
            engine.paddle_ocr = None
            engine.easy_ocr = None
            engine.languages = ['en']
            engine.use_gpu = False
            
            result = engine.process(sample_image)
            
            assert result["text"] == ""
            assert result["confidence"] == 0.0
            assert result["engine"] == "none"
    
    def test_process_specific_engine_paddle(self, ocr_engine, sample_image):
        """Test processing with specific engine (paddleocr)"""
        if ocr_engine.paddle_ocr:
            result = ocr_engine.process(sample_image, engine="paddleocr")
            assert result["engine"] == "paddleocr"
    
    def test_process_specific_engine_easyocr(self, ocr_engine, sample_image):
        """Test processing with specific engine (easyocr)"""
        if ocr_engine.easy_ocr:
            result = ocr_engine.process(sample_image, engine="easyocr")
            assert result["engine"] == "easyocr"
    
    def test_processing_time_measured(self, ocr_engine, sample_image):
        """Test that processing time is measured"""
        result = ocr_engine.process(sample_image)
        assert result["processing_time"] >= 0


class TestOCREngineMocked:
    """Test OCR Engine with mocked backends"""
    
    @pytest.fixture
    def sample_image(self):
        """Create a sample test image"""
        return np.ones((100, 100, 3), dtype=np.uint8) * 255
    
    def test_paddle_extraction(self, sample_image):
        """Test PaddleOCR text extraction with mock"""
        engine = OCREngine.__new__(OCREngine)
        engine.languages = ['en']
        engine.use_gpu = False
        engine.easy_ocr = None
        
        # Mock PaddleOCR
        mock_paddle = MagicMock()
        mock_paddle.ocr.return_value = [[
            [[[0, 0], [100, 0], [100, 30], [0, 30]], ("Test text", 0.95)],
            [[[0, 40], [100, 40], [100, 70], [0, 70]], ("More text", 0.90)]
        ]]
        engine.paddle_ocr = mock_paddle
        
        text, confidence = engine.process_with_paddle(sample_image)
        
        assert "Test text" in text
        assert "More text" in text
        assert confidence > 0.9
    
    def test_easyocr_extraction(self, sample_image):
        """Test EasyOCR text extraction with mock"""
        engine = OCREngine.__new__(OCREngine)
        engine.languages = ['en']
        engine.use_gpu = False
        engine.paddle_ocr = None
        
        # Mock EasyOCR
        mock_easy = MagicMock()
        mock_easy.readtext.return_value = [
            ([[0, 0], [100, 0], [100, 30], [0, 30]], "Easy text", 0.88),
            ([[0, 40], [100, 40], [100, 70], [0, 70]], "OCR text", 0.92)
        ]
        engine.easy_ocr = mock_easy
        
        text, confidence = engine.process_with_easyocr(sample_image)
        
        assert "Easy text" in text
        assert "OCR text" in text
        assert confidence == pytest.approx(0.9, abs=0.1)
    
    def test_fallback_behavior(self, sample_image):
        """Test fallback when first engine fails"""
        engine = OCREngine.__new__(OCREngine)
        engine.languages = ['en']
        engine.use_gpu = False
        
        # Mock paddle to fail
        mock_paddle = MagicMock()
        mock_paddle.ocr.side_effect = RuntimeError("Paddle failed")
        engine.paddle_ocr = mock_paddle
        
        # Mock easyocr to succeed
        mock_easy = MagicMock()
        mock_easy.readtext.return_value = [
            ([[0, 0], [100, 0], [100, 30], [0, 30]], "Fallback text", 0.85)
        ]
        engine.easy_ocr = mock_easy
        
        result = engine.process(sample_image)
        
        assert result["engine"] == "easyocr"
        assert "Fallback text" in result["text"]


class TestGetOcrEngine:
    """Test get_ocr_engine singleton function"""
    
    def test_returns_ocr_engine(self):
        """Test that get_ocr_engine returns OCREngine instance"""
        engine = get_ocr_engine()
        assert isinstance(engine, OCREngine)
    
    def test_singleton_pattern(self):
        """Test that same instance is returned"""
        engine1 = get_ocr_engine()
        engine2 = get_ocr_engine()
        assert engine1 is engine2
