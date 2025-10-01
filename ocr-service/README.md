# OCR Microservice for Thai Bank Slip Verification

A high-performance Python-based OCR microservice for extracting and verifying data from Thai bank payment slips.

## 🎯 Features

- **Multi-Engine OCR**: PaddleOCR and EasyOCR with automatic fallback
- **Thai Language Support**: Full support for Thai and English text
- **Image Preprocessing**: Advanced preprocessing pipeline for improved accuracy
- **Smart Data Extraction**: RegEx-based extraction for bank slip fields
- **Async Processing**: Redis-based queue for batch processing
- **High Performance**: Processing time < 3s per image
- **REST API**: FastAPI-based RESTful API with automatic documentation
- **Caching**: Redis caching for results
- **Docker Support**: Fully containerized with Docker

## 🏗️ Architecture

```
ocr-service/
├── app/
│   ├── api/                    # API endpoints
│   │   └── endpoints.py        # FastAPI routes
│   ├── core/                   # Core configuration
│   │   └── config.py           # Settings and configuration
│   ├── models/                 # Data models
│   │   └── schemas.py          # Pydantic models
│   ├── services/               # Business logic
│   │   ├── ocr_service.py      # OCR engine integration
│   │   ├── redis_service.py    # Redis service
│   │   └── processing_service.py # Main processing logic
│   ├── utils/                  # Utilities
│   │   ├── image_preprocessing.py # Image processing
│   │   └── data_extraction.py  # Data extraction patterns
│   └── main.py                 # FastAPI application
├── tests/                      # Unit and integration tests
├── Dockerfile                  # Docker configuration
├── requirements.txt            # Python dependencies
├── .env.example                # Environment variables template
└── README.md                   # This file
```

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Start all services including OCR
docker-compose -f docker-compose.frontend.yml up -d

# OCR service will be available at http://localhost:8000
# API documentation at http://localhost:8000/docs
```

### Manual Setup

#### Prerequisites

- Python 3.12+
- Redis (optional, for caching)

#### Installation

```bash
# Navigate to OCR service directory
cd ocr-service

# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Copy environment configuration
cp .env.example .env

# Run the service
uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
```

## 📚 API Documentation

### Base URL
```
http://localhost:8000/api/ocr
```

### Endpoints

#### 1. Process Image

Upload and process a single image.

**Endpoint:** `POST /api/ocr/process`

**Request:**
```bash
curl -X POST "http://localhost:8000/api/ocr/process" \
  -F "file=@slip.jpg" \
  -F "preprocess=true" \
  -F "ocr_engine=paddleocr"
```

**Response:**
```json
{
  "job_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "completed",
  "message": "Processing completed"
}
```

#### 2. Get Processing Status

Check the status of a processing job.

**Endpoint:** `GET /api/ocr/status/{job_id}`

**Request:**
```bash
curl -X GET "http://localhost:8000/api/ocr/status/550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "job_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "completed",
  "progress": 100.0,
  "message": "Processing completed successfully"
}
```

#### 3. Get OCR Result

Retrieve the complete OCR result with extracted data.

**Endpoint:** `GET /api/ocr/result/{job_id}`

**Request:**
```bash
curl -X GET "http://localhost:8000/api/ocr/result/550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "job_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "completed",
  "raw_text": "ธนาคารกสิกรไทย...",
  "extracted_data": {
    "amount": 1500.00,
    "transaction_date": "01/10/2024",
    "transaction_time": "14:30:00",
    "reference_number": "REF123456789",
    "bank": {
      "name": "Kasikorn Bank",
      "code": "KBANK"
    },
    "sender_account": "123-4-56789-0",
    "receiver_account": "987-6-54321-0",
    "sender_name": null,
    "receiver_name": null
  },
  "confidence": 0.95,
  "ocr_engine": "paddleocr",
  "processing_time": 2.35,
  "error_message": null,
  "created_at": "2024-10-01T14:30:00Z",
  "updated_at": "2024-10-01T14:30:02Z"
}
```

#### 4. Batch Processing

Process multiple images at once.

**Endpoint:** `POST /api/ocr/batch`

**Request:**
```bash
curl -X POST "http://localhost:8000/api/ocr/batch" \
  -F "files=@slip1.jpg" \
  -F "files=@slip2.jpg" \
  -F "files=@slip3.jpg" \
  -F "preprocess=true"
```

**Response:**
```json
{
  "batch_id": "batch-550e8400-e29b-41d4-a716-446655440000",
  "job_ids": [
    "batch-550e8400-e29b-41d4-a716-446655440000_0",
    "batch-550e8400-e29b-41d4-a716-446655440000_1",
    "batch-550e8400-e29b-41d4-a716-446655440000_2"
  ],
  "total": 3,
  "message": "Batch processing completed. 3 images processed."
}
```

### Interactive API Documentation

FastAPI provides automatic interactive API documentation:

- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

## 🎨 Supported Banks

The service recognizes the following Thai banks:

- ✅ **Bangkok Bank** (ธนาคารกรุงเทพ) - BBL
- ✅ **Kasikorn Bank** (ธนาคารกสิกรไทย) - KBANK
- ✅ **Siam Commercial Bank** (ธนาคารไทยพาณิชย์) - SCB
- ✅ **Krungthai Bank** (ธนาคารกรุงไทย) - KTB
- ✅ **TMB Thanachart Bank** (ธนาคารทหารไทยธนชาต) - TTB
- ✅ **PromptPay** (พร้อมเพย์) - PROMPTPAY

## 🔧 Configuration

### Environment Variables

Create a `.env` file based on `.env.example`:

```bash
# Application Settings
APP_NAME=OCR Microservice
DEBUG=False

# API Settings
PORT=8000

# Redis Settings
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_CACHE_TTL=3600

# OCR Settings
OCR_CONFIDENCE_THRESHOLD=0.6

# Image Processing
MAX_IMAGE_SIZE=10485760  # 10MB
IMAGE_PREPROCESSING=True

# Logging
LOG_LEVEL=INFO
```

## 🖼️ Image Preprocessing Pipeline

The service applies the following preprocessing steps:

1. **Grayscale Conversion**: Convert to grayscale for better OCR
2. **Noise Reduction**: Remove image noise using Non-Local Means Denoising
3. **Adaptive Thresholding**: Improve text contrast
4. **Deskewing**: Correct image rotation/tilt
5. **Border Removal**: Remove unnecessary borders
6. **Contrast Enhancement**: Apply CLAHE (Contrast Limited Adaptive Histogram Equalization)

## 📊 Data Extraction Patterns

The service extracts the following data using RegEx patterns:

- **Amount**: THB amounts in various formats (฿1,500.00, 1500 บาท, etc.)
- **Date**: Multiple date formats (DD/MM/YYYY, DD-MM-YYYY, Thai dates)
- **Time**: Time formats (HH:MM:SS, HH:MM)
- **Reference Number**: Transaction reference numbers
- **Bank Name**: Thai and English bank names
- **Account Numbers**: Various account number formats
- **PromptPay**: Phone numbers and ID card numbers

## 🧪 Testing

### Unit Tests

```bash
# Run all tests
pytest

# Run with coverage
pytest --cov=app tests/

# Run specific test file
pytest tests/test_ocr_service.py
```

### Manual Testing

Use the provided test script:

```bash
# Test single image
python -m tests.manual_test --image path/to/slip.jpg

# Test batch processing
python -m tests.manual_test --batch path/to/images/*.jpg
```

## 📈 Performance

- **Processing Time**: < 3 seconds per image
- **Accuracy**: > 90% for clear images
- **Throughput**: Supports concurrent requests
- **Batch Processing**: Up to 10 images per batch (configurable)

## 🔍 Troubleshooting

### OCR Engine Not Available

```bash
# Check if PaddleOCR is installed
python -c "import paddleocr; print('PaddleOCR OK')"

# Check if EasyOCR is installed
python -c "import easyocr; print('EasyOCR OK')"

# Reinstall if needed
pip install paddleocr easyocr --force-reinstall
```

### Redis Connection Issues

```bash
# Check Redis connection
redis-cli ping

# Check Redis from Docker
docker-compose exec redis redis-cli ping

# View logs
docker-compose logs ocr-service
```

### Low Accuracy Issues

1. **Enable preprocessing**: Set `preprocess=true` in requests
2. **Check image quality**: Ensure images are clear and well-lit
3. **Try different OCR engine**: Specify `ocr_engine=easyocr` or `paddleocr`
4. **Check logs**: View processing logs for issues

### Memory Issues

```bash
# Reduce batch size in .env
BATCH_SIZE=5

# Reduce max image size
MAX_IMAGE_SIZE=5242880  # 5MB
```

## 🚢 Deployment

### Docker Deployment

```bash
# Build image
docker build -t ocr-service:latest .

# Run container
docker run -d \
  -p 8000:8000 \
  -e REDIS_HOST=redis \
  --name ocr-service \
  ocr-service:latest
```

### Production Considerations

1. **Use GPU**: Set `use_gpu=True` for faster processing
2. **Scale horizontally**: Deploy multiple instances behind load balancer
3. **Monitor resources**: OCR processing is CPU/GPU intensive
4. **Set appropriate timeouts**: Adjust based on image complexity
5. **Implement rate limiting**: Prevent abuse
6. **Secure API**: Add authentication/authorization

## 📝 API Integration Example

### Python

```python
import requests

# Upload and process image
with open("slip.jpg", "rb") as f:
    files = {"file": f}
    data = {"preprocess": True}
    response = requests.post(
        "http://localhost:8000/api/ocr/process",
        files=files,
        data=data
    )
    job_id = response.json()["job_id"]

# Get result
result = requests.get(f"http://localhost:8000/api/ocr/result/{job_id}")
print(result.json())
```

### JavaScript

```javascript
// Upload and process image
const formData = new FormData();
formData.append('file', fileInput.files[0]);
formData.append('preprocess', 'true');

const response = await fetch('http://localhost:8000/api/ocr/process', {
  method: 'POST',
  body: formData
});

const { job_id } = await response.json();

// Get result
const result = await fetch(`http://localhost:8000/api/ocr/result/${job_id}`);
const data = await result.json();
console.log(data);
```

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is part of the Slip Verification System and is licensed under the MIT License.

## 📞 Support

For issues and questions:
- GitHub Issues: [Create an issue](https://github.com/picthaisky/slip-verification-system/issues)
- Documentation: See main [PROJECT_README.md](../PROJECT_README.md)

---

**Built with ❤️ using Python, FastAPI, PaddleOCR, and EasyOCR**
