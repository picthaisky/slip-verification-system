# OCR Service - Quick Start Guide

Get the OCR service up and running in 5 minutes!

## 🚀 Method 1: Docker (Recommended)

### Start Everything

```bash
# From project root
docker-compose -f docker-compose.frontend.yml up -d ocr-service redis

# Check if running
docker ps | grep ocr
```

### Test the Service

```bash
# Check health
curl http://localhost:8000/health

# View API documentation
open http://localhost:8000/docs
```

### Process a Test Image

```bash
# Download a test image (or use your own)
curl -o test_slip.jpg https://example.com/sample-slip.jpg

# Process the image
curl -X POST "http://localhost:8000/api/ocr/process" \
  -F "file=@test_slip.jpg" \
  -F "preprocess=true"

# Get result (replace JOB_ID with the job_id from previous response)
curl http://localhost:8000/api/ocr/result/JOB_ID
```

## 🔧 Method 2: Manual Setup

### Prerequisites

- Python 3.12+
- Redis (optional but recommended)

### Setup

```bash
# Navigate to OCR service
cd ocr-service

# Create virtual environment
python3 -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Copy environment config
cp .env.example .env

# Start Redis (optional)
docker run -d -p 6379:6379 redis:7-alpine
# OR use system Redis
redis-server

# Run the service
uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
```

### Test

```bash
# In another terminal
python example_usage.py --health

# Process an image (if you have one)
python example_usage.py --image path/to/slip.jpg
```

## 📝 Quick API Reference

### Base URL
```
http://localhost:8000/api/ocr
```

### Endpoints

**1. Process Image**
```bash
POST /api/ocr/process
- Upload a slip image for OCR processing
- Returns: job_id for tracking
```

**2. Check Status**
```bash
GET /api/ocr/status/{job_id}
- Check processing status
- Returns: status, progress percentage
```

**3. Get Result**
```bash
GET /api/ocr/result/{job_id}
- Get complete OCR result
- Returns: extracted data, confidence, raw text
```

**4. Batch Process**
```bash
POST /api/ocr/batch
- Process multiple images at once
- Returns: batch_id and list of job_ids
```

## 🎯 Expected Output

After processing, you'll get data like:

```json
{
  "job_id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "completed",
  "extracted_data": {
    "amount": 1500.00,
    "transaction_date": "01/10/2024",
    "transaction_time": "14:30:45",
    "reference_number": "REF123456789",
    "bank": {
      "name": "Kasikorn Bank",
      "code": "KBANK"
    },
    "sender_account": "123-4-56789-0",
    "receiver_account": "987-6-54321-0"
  },
  "confidence": 0.95,
  "processing_time": 2.35
}
```

## 🏦 Supported Banks

- ✅ Bangkok Bank (BBL)
- ✅ Kasikorn Bank (KBANK)
- ✅ Siam Commercial Bank (SCB)
- ✅ Krungthai Bank (KTB)
- ✅ TMB Thanachart Bank (TTB)
- ✅ PromptPay

## ⚡ Performance Tips

1. **Enable Preprocessing**: Set `preprocess=true` for better accuracy
2. **Use Good Images**: Clear, well-lit images work best
3. **Try Both Engines**: Test with `ocr_engine=paddleocr` or `ocr_engine=easyocr`
4. **Check Confidence**: Values > 0.9 indicate high accuracy

## 🔍 Troubleshooting

### Service Won't Start

```bash
# Check if port is in use
lsof -i :8000

# Check logs
docker-compose logs ocr-service

# Restart service
docker-compose restart ocr-service
```

### Redis Connection Error

```bash
# Check Redis is running
docker ps | grep redis
redis-cli ping

# Service works without Redis (caching disabled)
```

### Low Accuracy

1. Enable preprocessing: `preprocess=true`
2. Ensure image quality is good
3. Try different OCR engine
4. Check logs for specific errors

### Import Errors

```bash
# Reinstall dependencies
pip install -r requirements.txt --force-reinstall

# Check Python version
python3 --version  # Should be 3.12+
```

## 📚 Next Steps

- Read full [README.md](README.md) for detailed documentation
- Explore [API docs](http://localhost:8000/docs) (interactive)
- Check [example_usage.py](example_usage.py) for code examples
- See main [PROJECT_README.md](../PROJECT_README.md) for system overview

## 💡 Tips

- Use the interactive API docs at `/docs` for testing
- Enable debug mode in `.env` for detailed logs
- Monitor processing time to optimize performance
- Cache frequently processed slips in Redis

## 🆘 Need Help?

- Check service health: `curl http://localhost:8000/health`
- View logs: `docker-compose logs -f ocr-service`
- Test with example script: `python example_usage.py --health`
- Report issues on GitHub

---

**Ready to integrate? See [README.md](README.md) for full API documentation!**
