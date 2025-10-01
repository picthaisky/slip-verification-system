# API Documentation

## Base URL

```
Development: http://localhost:5000/api/v1
Production: https://api.slipverification.com/api/v1
```

## Authentication

All endpoints (except health check) require JWT Bearer token authentication.

### Headers

```http
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

## Response Format

### Success Response

```json
{
  "data": { ... },
  "isSuccess": true
}
```

### Error Response

```json
{
  "message": "Error description",
  "errors": {
    "fieldName": ["Error message 1", "Error message 2"]
  },
  "isSuccess": false
}
```

## HTTP Status Codes

- `200 OK` - Request succeeded
- `201 Created` - Resource created successfully
- `204 No Content` - Request succeeded with no response body
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error

---

## Endpoints

### Slip Verification

#### 1. Upload and Verify Slip

Upload a payment slip image for verification.

**Endpoint:** `POST /slips/verify`

**Authorization:** Required (User, Admin)

**Request:**
```http
POST /api/v1/slips/verify
Content-Type: multipart/form-data

orderId: 123e4567-e89b-12d3-a456-426614174000
file: [binary data]
```

**Response:** `200 OK`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "orderId": "123e4567-e89b-12d3-a456-426614174001",
  "imagePath": "slips/abc123_slip.jpg",
  "amount": 1500.00,
  "transactionDate": "2025-10-01T10:30:00Z",
  "referenceNumber": "REF123456",
  "bankName": "Bangkok Bank",
  "status": "Pending",
  "createdAt": "2025-10-01T10:30:00Z"
}
```

**Errors:**
- `400` - Invalid file format or missing orderId
- `404` - Order not found

---

#### 2. Get Slip by ID

Retrieve slip verification details by ID.

**Endpoint:** `GET /slips/{id}`

**Authorization:** Required (User, Admin)

**Request:**
```http
GET /api/v1/slips/123e4567-e89b-12d3-a456-426614174000
```

**Response:** `200 OK`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "orderId": "123e4567-e89b-12d3-a456-426614174001",
  "imagePath": "slips/abc123_slip.jpg",
  "amount": 1500.00,
  "transactionDate": "2025-10-01T10:30:00Z",
  "referenceNumber": "REF123456",
  "bankName": "Bangkok Bank",
  "senderAccount": "xxx-x-xxxxx-x",
  "receiverAccount": "yyy-y-yyyyy-y",
  "status": "Verified",
  "rawOcrText": "...",
  "ocrConfidence": 0.95,
  "verifiedAt": "2025-10-01T10:32:00Z",
  "createdAt": "2025-10-01T10:30:00Z"
}
```

**Errors:**
- `404` - Slip not found

---

#### 3. Get Slips by Order ID

Retrieve all slips associated with an order.

**Endpoint:** `GET /slips/order/{orderId}`

**Authorization:** Required (User, Admin)

**Request:**
```http
GET /api/v1/slips/order/123e4567-e89b-12d3-a456-426614174001
```

**Response:** `200 OK`
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "orderId": "123e4567-e89b-12d3-a456-426614174001",
    "imagePath": "slips/abc123_slip.jpg",
    "amount": 1500.00,
    "status": "Verified",
    "createdAt": "2025-10-01T10:30:00Z"
  }
]
```

---

#### 4. Delete Slip

Soft delete a slip verification record.

**Endpoint:** `DELETE /slips/{id}`

**Authorization:** Required (Admin only)

**Request:**
```http
DELETE /api/v1/slips/123e4567-e89b-12d3-a456-426614174000
```

**Response:** `204 No Content`

**Errors:**
- `403` - Forbidden (insufficient permissions)
- `404` - Slip not found

---

### Order Management

#### 1. Get All Orders

Retrieve paginated list of orders.

**Endpoint:** `GET /orders`

**Authorization:** Required (User, Admin)

**Query Parameters:**
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10) - Items per page

**Request:**
```http
GET /api/v1/orders?page=1&pageSize=10
```

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174001",
      "orderNumber": "ORD-2025-001",
      "amount": 1500.00,
      "status": "PendingPayment",
      "userId": "123e4567-e89b-12d3-a456-426614174002",
      "description": "Product purchase",
      "createdAt": "2025-10-01T10:00:00Z",
      "paidAt": null
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 50,
  "totalPages": 5
}
```

---

#### 2. Get Order by ID

Retrieve order details by ID.

**Endpoint:** `GET /orders/{id}`

**Authorization:** Required (User, Admin)

**Request:**
```http
GET /api/v1/orders/123e4567-e89b-12d3-a456-426614174001
```

**Response:** `200 OK`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174001",
  "orderNumber": "ORD-2025-001",
  "amount": 1500.00,
  "status": "Paid",
  "userId": "123e4567-e89b-12d3-a456-426614174002",
  "description": "Product purchase",
  "notes": "Customer notes",
  "expectedPaymentDate": "2025-10-02T00:00:00Z",
  "paidAt": "2025-10-01T10:35:00Z",
  "createdAt": "2025-10-01T10:00:00Z"
}
```

**Errors:**
- `404` - Order not found

---

#### 3. Update Order Status

Update the status of an order.

**Endpoint:** `PUT /orders/{id}/status`

**Authorization:** Required (User, Admin)

**Request:**
```http
PUT /api/v1/orders/123e4567-e89b-12d3-a456-426614174001/status
Content-Type: application/json

"Paid"
```

**Valid Status Values:**
- `PendingPayment`
- `Paid`
- `Processing`
- `Completed`
- `Cancelled`
- `Refunded`

**Response:** `200 OK`
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174001",
  "orderNumber": "ORD-2025-001",
  "status": "Paid",
  "updatedAt": "2025-10-01T10:40:00Z"
}
```

**Errors:**
- `400` - Invalid status value
- `404` - Order not found

---

#### 4. Get Pending Payment Orders

Retrieve all orders awaiting payment.

**Endpoint:** `GET /orders/pending-payment`

**Authorization:** Required (User, Admin)

**Request:**
```http
GET /api/v1/orders/pending-payment
```

**Response:** `200 OK`
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174001",
    "orderNumber": "ORD-2025-001",
    "amount": 1500.00,
    "status": "PendingPayment",
    "expectedPaymentDate": "2025-10-02T00:00:00Z",
    "createdAt": "2025-10-01T10:00:00Z"
  }
]
```

---

### Health Check

#### Get Application Health

Check the health status of the application and its dependencies.

**Endpoint:** `GET /health`

**Authorization:** Not required

**Request:**
```http
GET /health
```

**Response:** `200 OK` (Healthy)
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0001234"
    },
    {
      "name": "npgsql",
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0123456"
    }
  ],
  "totalDuration": "00:00:00.0124690"
}
```

**Response:** `503 Service Unavailable` (Unhealthy)
```json
{
  "status": "Unhealthy",
  "checks": [
    {
      "name": "npgsql",
      "status": "Unhealthy",
      "description": "Connection failed",
      "duration": "00:00:05.0000000"
    }
  ],
  "totalDuration": "00:00:05.0000000"
}
```

---

## Rate Limiting

API requests are rate-limited to prevent abuse.

**Limits:**
- 100 requests per minute per user/IP
- Exceeding the limit returns `429 Too Many Requests`

**Response Headers:**
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1633104000
```

---

## Examples

### cURL Examples

**Upload Slip:**
```bash
curl -X POST http://localhost:5000/api/v1/slips/verify \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "orderId=123e4567-e89b-12d3-a456-426614174001" \
  -F "file=@/path/to/slip.jpg"
```

**Get Order:**
```bash
curl -X GET http://localhost:5000/api/v1/orders/123e4567-e89b-12d3-a456-426614174001 \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### JavaScript/Fetch Example

```javascript
// Upload slip
const formData = new FormData();
formData.append('orderId', '123e4567-e89b-12d3-a456-426614174001');
formData.append('file', fileInput.files[0]);

const response = await fetch('http://localhost:5000/api/v1/slips/verify', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
  },
  body: formData
});

const data = await response.json();
console.log(data);
```

---

## Swagger/OpenAPI

Interactive API documentation is available at:

**Development:** http://localhost:5000/swagger

The Swagger UI provides:
- Complete API reference
- Try-it-out functionality
- Schema definitions
- Example requests and responses

---

## Webhook Events (Future Feature)

Webhook notifications for order status changes and slip verification results.

**Event Types:**
- `order.payment_verified`
- `order.payment_failed`
- `order.status_changed`

---

## Support

For API support and questions:
- Email: api-support@slipverification.com
- GitHub Issues: https://github.com/yourusername/slip-verification-system/issues
