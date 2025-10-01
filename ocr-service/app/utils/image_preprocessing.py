import cv2
import numpy as np
from PIL import Image
import io
from typing import Union


class ImagePreprocessor:
    """Image preprocessing for OCR accuracy improvement"""
    
    @staticmethod
    def preprocess_image(image: Union[np.ndarray, bytes, Image.Image]) -> np.ndarray:
        """
        Complete preprocessing pipeline
        
        Args:
            image: Input image (numpy array, bytes, or PIL Image)
            
        Returns:
            Preprocessed image as numpy array
        """
        # Convert to numpy array if needed
        img = ImagePreprocessor._to_numpy(image)
        
        # Apply preprocessing steps
        img = ImagePreprocessor.grayscale(img)
        img = ImagePreprocessor.denoise(img)
        img = ImagePreprocessor.deskew(img)
        img = ImagePreprocessor.threshold(img)
        img = ImagePreprocessor.remove_borders(img)
        img = ImagePreprocessor.enhance_contrast(img)
        
        return img
    
    @staticmethod
    def _to_numpy(image: Union[np.ndarray, bytes, Image.Image]) -> np.ndarray:
        """Convert image to numpy array"""
        if isinstance(image, np.ndarray):
            return image
        elif isinstance(image, bytes):
            nparr = np.frombuffer(image, np.uint8)
            return cv2.imdecode(nparr, cv2.IMREAD_COLOR)
        elif isinstance(image, Image.Image):
            return cv2.cvtColor(np.array(image), cv2.COLOR_RGB2BGR)
        else:
            raise ValueError(f"Unsupported image type: {type(image)}")
    
    @staticmethod
    def grayscale(image: np.ndarray) -> np.ndarray:
        """Convert image to grayscale"""
        if len(image.shape) == 3:
            return cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
        return image
    
    @staticmethod
    def denoise(image: np.ndarray) -> np.ndarray:
        """Apply noise reduction"""
        return cv2.fastNlMeansDenoising(image, None, 10, 7, 21)
    
    @staticmethod
    def threshold(image: np.ndarray) -> np.ndarray:
        """Apply adaptive thresholding"""
        return cv2.adaptiveThreshold(
            image,
            255,
            cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
            cv2.THRESH_BINARY,
            11,
            2
        )
    
    @staticmethod
    def deskew(image: np.ndarray) -> np.ndarray:
        """Deskew image by detecting and correcting rotation"""
        coords = np.column_stack(np.where(image > 0))
        if len(coords) == 0:
            return image
            
        angle = cv2.minAreaRect(coords)[-1]
        
        # Adjust angle
        if angle < -45:
            angle = -(90 + angle)
        else:
            angle = -angle
        
        # Rotate image only if angle is significant
        if abs(angle) > 0.5:
            (h, w) = image.shape[:2]
            center = (w // 2, h // 2)
            M = cv2.getRotationMatrix2D(center, angle, 1.0)
            rotated = cv2.warpAffine(
                image,
                M,
                (w, h),
                flags=cv2.INTER_CUBIC,
                borderMode=cv2.BORDER_REPLICATE
            )
            return rotated
        
        return image
    
    @staticmethod
    def remove_borders(image: np.ndarray, border_size: int = 10) -> np.ndarray:
        """Remove borders from image"""
        h, w = image.shape[:2]
        return image[border_size:h-border_size, border_size:w-border_size]
    
    @staticmethod
    def enhance_contrast(image: np.ndarray) -> np.ndarray:
        """Enhance image contrast using CLAHE"""
        clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
        return clahe.apply(image)
    
    @staticmethod
    def resize_if_needed(image: np.ndarray, max_width: int = 2000, max_height: int = 2000) -> np.ndarray:
        """Resize image if it's too large"""
        h, w = image.shape[:2]
        
        if w > max_width or h > max_height:
            scale = min(max_width / w, max_height / h)
            new_w = int(w * scale)
            new_h = int(h * scale)
            return cv2.resize(image, (new_w, new_h), interpolation=cv2.INTER_AREA)
        
        return image
    
    @staticmethod
    def to_bytes(image: np.ndarray, format: str = "PNG") -> bytes:
        """Convert numpy array to bytes"""
        if len(image.shape) == 2:
            # Grayscale
            pil_image = Image.fromarray(image, mode='L')
        else:
            # Color
            pil_image = Image.fromarray(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
        
        img_byte_arr = io.BytesIO()
        pil_image.save(img_byte_arr, format=format)
        return img_byte_arr.getvalue()
