# API Documentation

Complete API reference for Slip Verification System.

**Base URL**: `http://localhost:5000/api/v1`

---

## Authentication

All endpoints (except `/auth/*`) require JWT Bearer token in header:

```
Authorization: Bearer <your_jwt_token>
```

### POST /auth/login

Login with email and password.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "name": "User Name"
  }
}
```

---

## Dashboard

### GET /dashboard/stats

Get dashboard statistics overview.

**Response:**
```json
{
  "totalTransactions": 1250,
  "totalRevenue": 4567890.00,
  "verifiedCount": 1089,
  "pendingCount": 125,
  "rejectedCount": 36,
  "successRate": 87.12,
  "averageProcessingTime": 2.5,
  "todayTransactions": 42,
  "todayRevenue": 156750.00
}
```

### GET /dashboard/recent-activities

Get recent activities list.

**Parameters:**
- `count` (optional): Number of activities to return (default: 10)

**Response:**
```json
[
  {
    "id": "guid",
    "type": "SlipVerification",
    "description": "Slip #REF1234 verified",
    "status": "Verified",
    "amount": 5000.00,
    "createdAt": "2024-12-07T10:30:00Z",
    "timeAgo": "2 hours ago",
    "icon": "check-circle",
    "color": "#4CAF50"
  }
]
```

### GET /dashboard/chart-data

Get chart data for visualizations.

**Parameters:**
- `period` (optional): "daily", "weekly", "monthly" (default: "daily")
- `count` (optional): Number of periods (default: 7)

**Response:**
```json
{
  "labels": ["Dec 01", "Dec 02", "Dec 03"],
  "datasets": [
    {
      "label": "Total Transactions",
      "data": [45, 52, 38],
      "backgroundColor": "rgba(33, 150, 243, 0.2)",
      "borderColor": "rgba(33, 150, 243, 1)"
    }
  ]
}
```

---

## Reports

### GET /reports/daily

Get daily transaction report.

**Parameters:**
- `date` (optional): Report date in ISO format (default: today)

**Response:**
```json
{
  "date": "2024-12-07",
  "totalTransactions": 42,
  "totalRevenue": 156750.00,
  "verifiedCount": 35,
  "pendingCount": 5,
  "rejectedCount": 2,
  "transactions": [
    {
      "id": "guid",
      "referenceNumber": "REF001",
      "amount": 5000.00,
      "status": "Verified",
      "bankName": "Bangkok Bank",
      "transactionDate": "2024-12-07T14:30:00Z",
      "createdAt": "2024-12-07T14:30:00Z"
    }
  ]
}
```

### GET /reports/monthly

Get monthly summary report.

**Parameters:**
- `year` (optional): Report year (default: current year)
- `month` (optional): Report month 1-12 (default: current month)

**Response:**
```json
{
  "year": 2024,
  "month": 12,
  "monthName": "December",
  "totalTransactions": 1250,
  "totalRevenue": 4567890.00,
  "successRate": 87.12,
  "dailyBreakdown": [
    { "date": "2024-12-01", "count": 45, "amount": 178500.00 }
  ],
  "bankBreakdown": [
    { "bankName": "Bangkok Bank", "transactionCount": 450, "totalAmount": 1567890.00, "percentage": 34.3 }
  ]
}
```

### POST /reports/export

Export report to CSV file.

**Request:**
```json
{
  "format": "csv",
  "startDate": "2024-12-01",
  "endDate": "2024-12-07",
  "columns": ["referenceNumber", "amount", "status", "bankName", "transactionDate"]
}
```

**Response:** File download (text/csv)

---

## Slips

### POST /slips/verify

Upload and verify a payment slip.

**Request:** `multipart/form-data`
- `file`: Slip image file (JPG, PNG, PDF)
- `orderId`: Order ID (GUID)

**Response:**
```json
{
  "id": "guid",
  "orderId": "guid",
  "amount": 5000.00,
  "transactionDate": "2024-12-07",
  "referenceNumber": "REF123456789",
  "bankName": "Bangkok Bank",
  "status": "Verified",
  "confidence": 0.95
}
```

### GET /slips/{id}

Get slip verification details.

### GET /slips/order/{orderId}

Get all slips for an order.

---

## Orders

### GET /orders

Get paginated list of orders.

**Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)

### GET /orders/{id}

Get order details.

### GET /orders/pending-payment

Get orders pending payment.

---

## Error Responses

All endpoints may return error responses:

```json
{
  "message": "Error description",
  "errors": {
    "fieldName": ["Validation error message"]
  }
}
```

**HTTP Status Codes:**
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `500` - Internal Server Error

---

*Last Updated: December 2024*
