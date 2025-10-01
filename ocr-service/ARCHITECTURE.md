# OCR Service Architecture

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Slip Verification System                     │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────┐         ┌──────────────────┐
│   Angular Web    │◄───────►│   .NET Backend   │
│   Frontend       │         │   API            │
│   Port: 4200     │         │   Port: 5000     │
└──────────────────┘         └────────┬─────────┘
                                      │
                                      │ HTTP API Calls
                                      │
                             ┌────────▼─────────┐
                             │  OCR Service     │
                             │  (Python/FastAPI)│
                             │  Port: 8000      │
                             └────────┬─────────┘
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
            ┌───────▼──────┐  ┌──────▼──────┐  ┌──────▼──────┐
            │ PaddleOCR    │  │  EasyOCR    │  │   Redis     │
            │   Engine     │  │   Engine    │  │  Cache/Queue│
            └──────────────┘  └─────────────┘  └─────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                          PostgreSQL                              │
│                    (Persistent Storage)                          │
└─────────────────────────────────────────────────────────────────┘
```

## OCR Service Flow

```
1. Image Upload
   ┌─────────────┐
   │   Client    │
   │  (Web/API)  │
   └──────┬──────┘
          │ POST /api/ocr/process
          │ (multipart/form-data)
          ▼
   ┌─────────────┐
   │  FastAPI    │
   │  Endpoint   │
   └──────┬──────┘
          │
          │
2. Preprocessing
   ┌──────▼──────┐
   │ Image Pre-  │
   │ processor   │
   │             │
   │ • Grayscale │
   │ • Denoise   │
   │ • Threshold │
   │ • Deskew    │
   │ • Border    │
   │ • Contrast  │
   └──────┬──────┘
          │
          │
3. OCR Processing
   ┌──────▼──────┐
   │ OCR Engine  │
   │             │
   │ Try:        │
   │ • PaddleOCR │
   │ Fallback:   │
   │ • EasyOCR   │
   └──────┬──────┘
          │
          │
4. Data Extraction
   ┌──────▼──────┐
   │ Data        │
   │ Extractor   │
   │             │
   │ • Amount    │
   │ • Date      │
   │ • Time      │
   │ • Bank      │
   │ • Account   │
   │ • Reference │
   └──────┬──────┘
          │
          │
5. Caching & Response
   ┌──────▼──────┐
   │   Redis     │
   │   Cache     │
   └──────┬──────┘
          │
          ▼
   ┌─────────────┐
   │  JSON       │
   │  Response   │
   └─────────────┘
```

## Data Flow

```
Input Image → Preprocessing → OCR → Extraction → Cache → Response
                                │
                                ├─ Raw Text
                                ├─ Confidence Score
                                └─ Structured Data
```

## Component Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        OCR Service (app/)                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐     │
│  │     API      │    │    Models    │    │    Core      │     │
│  │              │    │              │    │              │     │
│  │ • endpoints  │    │ • schemas    │    │ • config     │     │
│  │ • routes     │    │ • validation │    │ • settings   │     │
│  └──────┬───────┘    └──────────────┘    └──────────────┘     │
│         │                                                       │
│         │                                                       │
│  ┌──────▼───────────────────────────────────────┐             │
│  │              Services                        │             │
│  │                                               │             │
│  │  ┌──────────────┐  ┌──────────────┐         │             │
│  │  │ Processing   │  │     OCR      │         │             │
│  │  │   Service    │  │   Service    │         │             │
│  │  │              │  │              │         │             │
│  │  │ • Orchestrate│  │ • PaddleOCR  │         │             │
│  │  │ • Async      │  │ • EasyOCR    │         │             │
│  │  │ • Job Mgmt   │  │ • Fallback   │         │             │
│  │  └──────────────┘  └──────────────┘         │             │
│  │                                               │             │
│  │  ┌──────────────┐                            │             │
│  │  │    Redis     │                            │             │
│  │  │   Service    │                            │             │
│  │  │              │                            │             │
│  │  │ • Cache      │                            │             │
│  │  │ • Queue      │                            │             │
│  │  └──────────────┘                            │             │
│  └───────────────────────────────────────────────┘             │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                      Utilities                          │  │
│  │                                                          │  │
│  │  ┌──────────────┐        ┌──────────────┐             │  │
│  │  │    Image     │        │     Data     │             │  │
│  │  │  Processing  │        │  Extraction  │             │  │
│  │  │              │        │              │             │  │
│  │  │ • Preprocess │        │ • RegEx      │             │  │
│  │  │ • Transform  │        │ • Patterns   │             │  │
│  │  │ • Enhance    │        │ • Validation │             │  │
│  │  └──────────────┘        └──────────────┘             │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Technology Stack

```
┌─────────────────────────────────────────────────────────────────┐
│                      OCR Service Stack                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Framework:        FastAPI 0.115.0                              │
│  Language:         Python 3.12                                  │
│                                                                  │
│  OCR Engines:                                                   │
│  • PaddleOCR 2.8.1  (Primary)                                  │
│  • EasyOCR 1.7.2    (Fallback)                                 │
│                                                                  │
│  Image Processing:                                              │
│  • OpenCV 4.10.0                                               │
│  • Pillow 11.0.0                                               │
│  • NumPy 1.26.4                                                │
│  • scikit-image 0.24.0                                         │
│                                                                  │
│  Queue & Cache:                                                 │
│  • Redis 5.2.0                                                 │
│                                                                  │
│  Logging:                                                       │
│  • Loguru 0.7.2                                                │
│                                                                  │
│  Validation:                                                    │
│  • Pydantic 2.9.2                                              │
│                                                                  │
│  Testing:                                                       │
│  • Pytest 8.3.3                                                │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Docker Compose                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │   Frontend   │  │   Backend    │  │ OCR Service  │         │
│  │   :4200      │  │   :5000      │  │   :8000      │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
│                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │  PostgreSQL  │  │    Redis     │  │   pgAdmin    │         │
│  │   :5432      │  │   :6379      │  │   :5050      │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
│                                                                  │
│  Network: slip-verification-network                             │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## API Endpoints

```
POST   /api/ocr/process          # Process single image
GET    /api/ocr/status/{job_id}  # Check processing status
GET    /api/ocr/result/{job_id}  # Get OCR result
POST   /api/ocr/batch            # Batch process images
GET    /health                    # Health check
GET    /docs                      # API documentation (Swagger)
GET    /redoc                     # API documentation (ReDoc)
```

## Supported Banks

```
┌─────────────────────────────────────────────────────────────────┐
│                      Thai Banks Support                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ✓ Bangkok Bank            (BBL)     ธนาคารกรุงเทพ             │
│  ✓ Kasikorn Bank           (KBANK)   ธนาคารกสิกรไทย            │
│  ✓ Siam Commercial Bank    (SCB)     ธนาคารไทยพาณิชย์           │
│  ✓ Krungthai Bank          (KTB)     ธนาคารกรุงไทย             │
│  ✓ TMB Thanachart Bank     (TTB)     ธนาคารทหารไทยธนชาต        │
│  ✓ PromptPay               (PP)      พร้อมเพย์                 │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```
