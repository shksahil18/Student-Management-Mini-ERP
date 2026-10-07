# API Specification & Contracts

## Base URL
`/api`

## Response Wrappers
### Standard Success Envelope
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

### Standard Error Envelope
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "email": ["The email format is invalid."]
  },
  "traceId": "00-4b2a8..."
}
```

---

## 1. Authentication Endpoints

### `POST /api/auth/login`
- **Auth:** Public
- **Request Body:**
  ```json
  {
    "username": "admin",
    "password": "Password123!"
  }
  ```
- **Response `200 OK`:**
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@erp.local",
      "role": "Admin"
    }
  }
  ```

### `GET /api/auth/me`
- **Auth:** Bearer Token
- **Response `200 OK`:** Current authenticated user profile.

---

## 2. Student Endpoints

### `GET /api/students`
- **Auth:** Bearer Token
- **Query Parameters:**
  - `page` (int, default: 1)
  - `pageSize` (int, default: 10)
  - `search` (string, optional: code, name, or email)
  - `course` (string, optional)
  - `isActive` (bool, optional)
- **Response `200 OK`:**
  ```json
  {
    "items": [
      {
        "id": 1,
        "studentCode": "STU-2026-00001",
        "fullName": "Aarav Sharma",
        "email": "aarav.sharma@example.com",
        "phone": "+91 9876543210",
        "course": "MCA",
        "isActive": true,
        "admissionDate": "2026-01-15"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalItems": 45,
    "totalPages": 5
  }
  ```

### `GET /api/students/{id}`
- **Auth:** Bearer Token
- **Response `200 OK`:** Complete student details including fee structure and payment history.

### `POST /api/students`
- **Auth:** Bearer Token (Admin)
- **Request Body:**
  ```json
  {
    "firstName": "Rohan",
    "lastName": "Verma",
    "dateOfBirth": "2003-08-20",
    "gender": "Male",
    "email": "rohan.verma@example.com",
    "phone": "9876543219",
    "address": "42 Park Street, Pune",
    "admissionDate": "2026-07-01",
    "course": "B.Tech Computer Science"
  }
  ```
- **Response `201 Created`**

### `PUT /api/students/{id}`
- **Auth:** Bearer Token (Admin)
- **Request Body:** Updates non-key fields.
- **Response `200 OK`**

### `PATCH /api/students/{id}/toggle-status`
- **Auth:** Bearer Token (Admin)
- **Response `200 OK`:** Toggles `isActive` state (soft deactivation / activation).

---

## 3. Fee & Payment Endpoints

### `GET /api/fees/student/{studentId}`
- **Auth:** Bearer Token
- **Response `200 OK`:** List of fee records and corresponding payment receipts.

### `POST /api/fees`
- **Auth:** Bearer Token (Admin)
- **Request Body:**
  ```json
  {
    "studentId": 1,
    "totalAmount": 75000.00,
    "dueDate": "2026-09-30"
  }
  ```
- **Response `201 Created`**

### `POST /api/fees/{feeId}/payments`
- **Auth:** Bearer Token (Admin, Staff)
- **Request Body:**
  ```json
  {
    "amount": 25000.00,
    "paymentMethod": "UPI",
    "notes": "Semester 1 initial installment"
  }
  ```
- **Response `201 Created`:**
  ```json
  {
    "receiptNumber": "REC-2026-000042",
    "amount": 25000.00,
    "paymentDate": "2026-10-07T12:00:00Z",
    "remainingOutstanding": 50000.00,
    "feeStatus": "Partial"
  }
  ```

### `GET /api/payments/{receiptNumber}`
- **Auth:** Bearer Token
- **Response `200 OK`:** Printable receipt data breakdown.

---

## 4. Dashboard Endpoints

### `GET /api/dashboard/summary`
- **Auth:** Bearer Token
- **Response `200 OK`:**
  ```json
  {
    "totalStudents": 120,
    "activeStudents": 115,
    "inactiveStudents": 5,
    "totalFeesBilled": 7500000.00,
    "totalFeesCollected": 5200000.00,
    "totalOutstandingFees": 2300000.00,
    "recentPayments": [ ... ],
    "recentAdmissions": [ ... ]
  }
  ```
