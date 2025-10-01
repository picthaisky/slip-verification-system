import pytest
import numpy as np
import cv2
from PIL import Image
from app.utils.image_preprocessing import ImagePreprocessor


class TestImagePreprocessor:
    """Test image preprocessing functions"""
    
    @pytest.fixture
    def sample_image(self):
        """Create a sample image for testing"""
        # Create a simple test image (100x100 white image with black text)
        img = np.ones((100, 100, 3), dtype=np.uint8) * 255
        cv2.putText(img, "TEST", (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 0), 2)
        return img
    
    def test_to_numpy_from_numpy(self, sample_image):
        """Test conversion from numpy array"""
        result = ImagePreprocessor._to_numpy(sample_image)
        assert isinstance(result, np.ndarray)
        assert result.shape == sample_image.shape
    
    def test_to_numpy_from_pil(self, sample_image):
        """Test conversion from PIL Image"""
        pil_image = Image.fromarray(cv2.cvtColor(sample_image, cv2.COLOR_BGR2RGB))
        result = ImagePreprocessor._to_numpy(pil_image)
        assert isinstance(result, np.ndarray)
        assert len(result.shape) == 3
    
    def test_to_numpy_from_bytes(self, sample_image):
        """Test conversion from bytes"""
        _, buffer = cv2.imencode('.jpg', sample_image)
        image_bytes = buffer.tobytes()
        result = ImagePreprocessor._to_numpy(image_bytes)
        assert isinstance(result, np.ndarray)
        assert len(result.shape) == 3
    
    def test_grayscale(self, sample_image):
        """Test grayscale conversion"""
        gray = ImagePreprocessor.grayscale(sample_image)
        assert len(gray.shape) == 2  # Should be 2D
        assert gray.dtype == np.uint8
    
    def test_grayscale_already_gray(self, sample_image):
        """Test grayscale on already grayscale image"""
        gray = cv2.cvtColor(sample_image, cv2.COLOR_BGR2GRAY)
        result = ImagePreprocessor.grayscale(gray)
        assert len(result.shape) == 2
        np.testing.assert_array_equal(result, gray)
    
    def test_threshold(self, sample_image):
        """Test adaptive thresholding"""
        gray = ImagePreprocessor.grayscale(sample_image)
        thresholded = ImagePreprocessor.threshold(gray)
        assert thresholded.shape == gray.shape
        # Check it's binary (only 0 and 255)
        unique_values = np.unique(thresholded)
        assert len(unique_values) <= 2
    
    def test_enhance_contrast(self, sample_image):
        """Test contrast enhancement"""
        gray = ImagePreprocessor.grayscale(sample_image)
        enhanced = ImagePreprocessor.enhance_contrast(gray)
        assert enhanced.shape == gray.shape
        assert enhanced.dtype == gray.dtype
    
    def test_resize_if_needed_no_resize(self):
        """Test resize when image is small enough"""
        small_image = np.ones((100, 100), dtype=np.uint8)
        result = ImagePreprocessor.resize_if_needed(small_image, max_width=200, max_height=200)
        assert result.shape == small_image.shape
    
    def test_resize_if_needed_resize(self):
        """Test resize when image is too large"""
        large_image = np.ones((3000, 3000), dtype=np.uint8)
        result = ImagePreprocessor.resize_if_needed(large_image, max_width=2000, max_height=2000)
        assert result.shape[0] <= 2000
        assert result.shape[1] <= 2000
    
    def test_remove_borders(self, sample_image):
        """Test border removal"""
        gray = ImagePreprocessor.grayscale(sample_image)
        result = ImagePreprocessor.remove_borders(gray, border_size=5)
        assert result.shape[0] == gray.shape[0] - 10  # 5 pixels from each side
        assert result.shape[1] == gray.shape[1] - 10
    
    def test_to_bytes(self, sample_image):
        """Test conversion to bytes"""
        gray = ImagePreprocessor.grayscale(sample_image)
        image_bytes = ImagePreprocessor.to_bytes(gray, format="PNG")
        assert isinstance(image_bytes, bytes)
        assert len(image_bytes) > 0
    
    def test_preprocess_pipeline(self, sample_image):
        """Test complete preprocessing pipeline"""
        processed = ImagePreprocessor.preprocess_image(sample_image)
        assert isinstance(processed, np.ndarray)
        # Should be grayscale after preprocessing
        assert len(processed.shape) == 2
        # Should be thresholded (binary)
        unique_values = np.unique(processed)
        assert len(unique_values) <= 2
    
    def test_deskew_no_rotation_needed(self):
        """Test deskew when no rotation is needed"""
        straight_image = np.ones((100, 100), dtype=np.uint8) * 255
        cv2.rectangle(straight_image, (20, 20), (80, 80), 0, -1)
        result = ImagePreprocessor.deskew(straight_image)
        assert result.shape == straight_image.shape
