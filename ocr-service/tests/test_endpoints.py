import pytest
from fastapi.testclient import TestClient
from unittest.mock import patch, MagicMock, AsyncMock
import io
from PIL import Image
import numpy as np

from app.main import app
from app.models.schemas import ProcessingStatus, OcrResult


# Create test client
client = TestClient(app)


class TestHealthEndpoint:
    """Test health check endpoint"""
    
    def test_health_check(self):
        """Test health endpoint returns valid response"""
        response = client.get("/health")
        assert response.status_code == 200
        
        data = response.json()
        assert "status" in data
        assert "version" in data
        assert "timestamp" in data
        assert data["status"] in ["healthy", "degraded"]


class TestRootEndpoint:
    """Test root endpoint"""
    
    def test_root(self):
        """Test root endpoint returns service info"""
        response = client.get("/")
        assert response.status_code == 200
        
        data = response.json()
        assert "service" in data
        assert "version" in data
        assert "status" in data
        assert data["status"] == "running"


class TestProcessEndpoint:
    """Test /api/ocr/process endpoint"""
    
    @pytest.fixture
    def sample_image_bytes(self):
        """Create sample image as bytes"""
        img = Image.new('RGB', (100, 100), color='white')
        img_byte_arr = io.BytesIO()
        img.save(img_byte_arr, format='JPEG')
        img_byte_arr.seek(0)
        return img_byte_arr.getvalue()
    
    def test_process_no_file(self):
        """Test process endpoint without file"""
        response = client.post("/api/ocr/process")
        assert response.status_code == 422  # Unprocessable Entity
    
    def test_process_invalid_file_type(self):
        """Test process endpoint with invalid file type"""
        response = client.post(
            "/api/ocr/process",
            files={"file": ("test.txt", b"not an image", "text/plain")}
        )
        assert response.status_code == 400
    
    def test_process_empty_file(self):
        """Test process endpoint with empty file"""
        response = client.post(
            "/api/ocr/process",
            files={"file": ("test.jpg", b"", "image/jpeg")}
        )
        assert response.status_code == 400
    
    @patch('app.api.endpoints.get_processing_service')
    def test_process_valid_image(self, mock_get_service, sample_image_bytes):
        """Test process endpoint with valid image"""
        # Mock the processing service
        mock_service = MagicMock()
        mock_result = MagicMock()
        mock_result.job_id = "test-job-123"
        mock_result.status = ProcessingStatus.COMPLETED
        
        mock_service.process_image = AsyncMock(return_value=mock_result)
        mock_get_service.return_value = mock_service
        
        response = client.post(
            "/api/ocr/process",
            files={"file": ("test.jpg", sample_image_bytes, "image/jpeg")},
            data={"preprocess": "true"}
        )
        
        assert response.status_code == 200
        data = response.json()
        assert "job_id" in data
        assert data["job_id"] == "test-job-123"
    
    @patch('app.api.endpoints.get_processing_service')
    def test_process_with_ocr_engine_param(self, mock_get_service, sample_image_bytes):
        """Test process endpoint with specific OCR engine"""
        mock_service = MagicMock()
        mock_result = MagicMock()
        mock_result.job_id = "test-job-456"
        mock_result.status = ProcessingStatus.COMPLETED
        
        mock_service.process_image = AsyncMock(return_value=mock_result)
        mock_get_service.return_value = mock_service
        
        response = client.post(
            "/api/ocr/process",
            files={"file": ("test.jpg", sample_image_bytes, "image/jpeg")},
            data={"preprocess": "true", "ocr_engine": "paddleocr"}
        )
        
        assert response.status_code == 200


class TestStatusEndpoint:
    """Test /api/ocr/status/{job_id} endpoint"""
    
    @patch('app.api.endpoints.get_processing_service')
    def test_status_not_found(self, mock_get_service):
        """Test status endpoint with non-existent job"""
        mock_service = MagicMock()
        mock_service.get_result.return_value = None
        mock_get_service.return_value = mock_service
        
        response = client.get("/api/ocr/status/non-existent-job")
        assert response.status_code == 404
    
    @patch('app.api.endpoints.get_processing_service')
    def test_status_pending(self, mock_get_service):
        """Test status endpoint with pending job"""
        mock_service = MagicMock()
        mock_result = MagicMock()
        mock_result.status = ProcessingStatus.PENDING
        mock_service.get_result.return_value = mock_result
        mock_get_service.return_value = mock_service
        
        response = client.get("/api/ocr/status/pending-job")
        assert response.status_code == 200
        
        data = response.json()
        assert data["progress"] == 0.0
    
    @patch('app.api.endpoints.get_processing_service')
    def test_status_completed(self, mock_get_service):
        """Test status endpoint with completed job"""
        mock_service = MagicMock()
        mock_result = MagicMock()
        mock_result.status = ProcessingStatus.COMPLETED
        mock_result.error_message = None
        mock_service.get_result.return_value = mock_result
        mock_get_service.return_value = mock_service
        
        response = client.get("/api/ocr/status/completed-job")
        assert response.status_code == 200
        
        data = response.json()
        assert data["progress"] == 100.0


class TestResultEndpoint:
    """Test /api/ocr/result/{job_id} endpoint"""
    
    @patch('app.api.endpoints.get_processing_service')
    def test_result_not_found(self, mock_get_service):
        """Test result endpoint with non-existent job"""
        mock_service = MagicMock()
        mock_service.get_result.return_value = None
        mock_get_service.return_value = mock_service
        
        response = client.get("/api/ocr/result/non-existent-job")
        assert response.status_code == 404
    
    @patch('app.api.endpoints.get_processing_service')
    def test_result_success(self, mock_get_service):
        """Test result endpoint with valid job"""
        from datetime import datetime
        
        mock_service = MagicMock()
        mock_result = OcrResult(
            job_id="test-job",
            status=ProcessingStatus.COMPLETED,
            raw_text="Test OCR text",
            extracted_data={
                "amount": 1500.00,
                "transaction_date": "01/01/2024",
                "bank": {"name": "Test Bank", "code": "TEST"}
            },
            confidence=0.95,
            ocr_engine="paddleocr",
            processing_time=1.5,
            created_at=datetime.utcnow(),
            updated_at=datetime.utcnow()
        )
        mock_service.get_result.return_value = mock_result
        mock_get_service.return_value = mock_service
        
        response = client.get("/api/ocr/result/test-job")
        assert response.status_code == 200


class TestBatchEndpoint:
    """Test /api/ocr/batch endpoint"""
    
    @pytest.fixture
    def sample_images(self):
        """Create multiple sample images"""
        images = []
        for i in range(3):
            img = Image.new('RGB', (100, 100), color='white')
            img_byte_arr = io.BytesIO()
            img.save(img_byte_arr, format='JPEG')
            img_byte_arr.seek(0)
            images.append((f"test{i}.jpg", img_byte_arr.getvalue(), "image/jpeg"))
        return images
    
    def test_batch_no_files(self):
        """Test batch endpoint without files"""
        response = client.post("/api/ocr/batch")
        assert response.status_code == 422
    
    @patch('app.api.endpoints.get_processing_service')
    def test_batch_valid_images(self, mock_get_service, sample_images):
        """Test batch endpoint with valid images"""
        mock_service = MagicMock()
        mock_results = [
            MagicMock(job_id=f"batch-job-{i}") for i in range(3)
        ]
        mock_service.process_batch = AsyncMock(return_value=mock_results)
        mock_get_service.return_value = mock_service
        
        files = [("files", img) for img in sample_images]
        response = client.post(
            "/api/ocr/batch",
            files=files,
            data={"preprocess": "true"}
        )
        
        assert response.status_code == 200
        data = response.json()
        assert "batch_id" in data
        assert "job_ids" in data
        assert data["total"] == 3
