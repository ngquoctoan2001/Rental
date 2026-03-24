# RentalOS — 03. API Conventions & Endpoints Đầy đủ

---

## Base URL

```
Production : https://api.rentalos.vn/api/v1
Local      : http://localhost:5000/api/v1
```

---

## Authentication

```
Authorization: Bearer {access_token}
Content-Type: application/json
```

Public endpoints (không cần token):
- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/forgot-password`
- `POST /auth/reset-password`
- `GET  /pay/{token}`
- `POST /payments/momo/webhook`
- `POST /payments/vnpay/webhook`
- `GET  /payments/vnpay/return`
- `GET  /health`

---

## Response Format

```json
// Success (single)
{ "success": true, "data": { ... } }

// Success (list)
{
  "success": true,
  "data": [ ... ],
  "meta": { "page": 1, "pageSize": 20, "total": 150, "totalPages": 8 }
}

// Error
{
  "success": false,
  "error": { "code": "ROOM_NOT_FOUND", "message": "Không tìm thấy phòng", "details": [] }
}
```

---

## Pagination & Sorting

```
?page=1&pageSize=20&sortBy=createdAt&sortDir=desc&search=keyword
```

---

## Error Codes đầy đủ

| Code | HTTP | Mô tả |
|------|------|-------|
| `VALIDATION_ERROR` | 400 | Dữ liệu đầu vào không hợp lệ |
| `AUTH_INVALID_CREDENTIALS` | 401 | Sai email/mật khẩu |
| `AUTH_TOKEN_EXPIRED` | 401 | Token hết hạn |
| `AUTH_TOKEN_INVALID` | 401 | Token không hợp lệ |
| `AUTH_FORBIDDEN` | 403 | Không có quyền |
| `AUTH_INVITE_EXPIRED` | 400 | Link mời đã hết hạn |
| `TENANT_NOT_FOUND` | 404 | Tenant không tồn tại |
| `TENANT_INACTIVE` | 403 | Tenant bị vô hiệu hóa |
| `TENANT_PLAN_LIMIT` | 402 | Vượt giới hạn plan |
| `TENANT_TRIAL_EXPIRED` | 402 | Hết trial, cần mua gói |
| `PROPERTY_NOT_FOUND` | 404 | Nhà trọ không tồn tại |
| `ROOM_NOT_FOUND` | 404 | Phòng không tồn tại |
| `ROOM_NOT_AVAILABLE` | 409 | Phòng không trống |
| `ROOM_HAS_ACTIVE_CONTRACT` | 409 | Phòng đang có hợp đồng |
| `CUSTOMER_NOT_FOUND` | 404 | Khách thuê không tồn tại |
| `CUSTOMER_BLACKLISTED` | 409 | Khách bị blacklist |
| `CONTRACT_NOT_FOUND` | 404 | Hợp đồng không tồn tại |
| `CONTRACT_NOT_ACTIVE` | 409 | Hợp đồng không đang hoạt động |
| `INVOICE_NOT_FOUND` | 404 | Hóa đơn không tồn tại |
| `INVOICE_ALREADY_PAID` | 409 | Hóa đơn đã thanh toán |
| `INVOICE_ALREADY_EXISTS` | 409 | Đã có hóa đơn tháng này |
| `INVOICE_CANCELLED` | 409 | Hóa đơn đã hủy |
| `PAYMENT_LINK_EXPIRED` | 410 | Link thanh toán hết hạn |
| `PAYMENT_LINK_INVALID` | 404 | Link thanh toán không tồn tại |
| `PAYMENT_VERIFICATION_FAILED` | 400 | Chữ ký webhook không hợp lệ |
| `PAYMENT_DUPLICATE` | 200 | Giao dịch đã xử lý |
| `FILE_TOO_LARGE` | 413 | File quá lớn (>10MB) |
| `FILE_TYPE_INVALID` | 400 | Loại file không được phép |
| `OCR_FAILED` | 422 | Không nhận diện được CCCD |
| `STAFF_NOT_FOUND` | 404 | Nhân viên không tồn tại |
| `INTERNAL_ERROR` | 500 | Lỗi hệ thống |

---

## Tổng hợp tất cả API Endpoints (~130 endpoints)

### AUTH (6)
```
POST   /auth/register
POST   /auth/login
POST   /auth/refresh
POST   /auth/forgot-password
POST   /auth/reset-password
PUT    /auth/change-password
```

### PROPERTIES (8)
```
GET    /properties
GET    /properties/{id}
GET    /properties/{id}/stats
GET    /properties/{id}/rooms
POST   /properties
PUT    /properties/{id}
DELETE /properties/{id}
POST   /properties/{id}/images
```

### ROOMS (12)
```
GET    /rooms
GET    /rooms/{id}
GET    /rooms/{id}/history
GET    /rooms/{id}/meter-readings
GET    /rooms/{id}/qrcode
GET    /rooms/available
POST   /rooms
PUT    /rooms/{id}
PATCH  /rooms/{id}/status
DELETE /rooms/{id}
POST   /rooms/{id}/images
POST   /rooms/bulk-create
```

### METER READINGS (5) — MỚI riêng biệt
```
GET    /meter-readings?roomId=&month=
GET    /meter-readings/{id}
POST   /meter-readings
PUT    /meter-readings/{id}
DELETE /meter-readings/{id}
```

### CUSTOMERS (10)
```
GET    /customers
GET    /customers/{id}
GET    /customers/{id}/contracts
GET    /customers/{id}/invoices
GET    /customers/search?q=
POST   /customers
PUT    /customers/{id}
POST   /customers/ocr
PATCH  /customers/{id}/blacklist
POST   /customers/{id}/images
```

### CONTRACTS (11)
```
GET    /contracts
GET    /contracts/{id}
GET    /contracts/{id}/invoices
GET    /contracts/{id}/co-tenants
GET    /contracts/expiring
POST   /contracts
PUT    /contracts/{id}
PATCH  /contracts/{id}/terminate
POST   /contracts/{id}/renew
GET    /contracts/{id}/pdf
POST   /contracts/{id}/sign
```

### CO-TENANTS (4) — MỚI
```
GET    /contracts/{id}/co-tenants
POST   /contracts/{id}/co-tenants
PUT    /contracts/{id}/co-tenants/{cotId}
DELETE /contracts/{id}/co-tenants/{cotId}
```

### INVOICES (12)
```
GET    /invoices
GET    /invoices/{id}
GET    /invoices/{id}/transactions
GET    /invoices/overdue
GET    /invoices/pending-meter
POST   /invoices
POST   /invoices/bulk-generate
PUT    /invoices/{id}
PATCH  /invoices/{id}/cancel
POST   /invoices/{id}/send
POST   /invoices/{id}/resend
GET    /invoices/{id}/pdf
GET    /invoices/{id}/payment-link
```

### PAYMENTS (7)
```
GET    /pay/{token}                   ← public
POST   /payments/momo/create
POST   /payments/momo/webhook         ← public
POST   /payments/vnpay/create
POST   /payments/vnpay/webhook        ← public
GET    /payments/vnpay/return         ← public
GET    /payments/methods              ← methods available cho tenant
```

### TRANSACTIONS (6)
```
GET    /transactions
GET    /transactions/{id}
GET    /transactions/summary
POST   /transactions/cash
POST   /transactions/deposit-refund   ← MỚI: hoàn cọc
GET    /transactions/export
```

### REPORTS (8) — MỚI
```
GET    /reports/dashboard
GET    /reports/revenue
GET    /reports/occupancy
GET    /reports/collection-rate
GET    /reports/overdue-trend
GET    /reports/top-rooms
GET    /reports/monthly-summary
GET    /reports/export
```

### NOTIFICATIONS (4)
```
GET    /notifications
PATCH  /notifications/{id}/read
PATCH  /notifications/read-all
GET    /notifications/logs            ← MỚI: Zalo/SMS logs
```

### STAFF (9)
```
GET    /staff
GET    /staff/{id}
GET    /staff/{id}/activity-log
GET    /staff/invite/verify/{token}   ← MỚI: kiểm tra link mời
POST   /staff
POST   /staff/accept-invite           ← MỚI: nhân viên chấp nhận mời
PUT    /staff/{id}
PATCH  /staff/{id}/permissions
PATCH  /staff/{id}/deactivate
```

### SETTINGS (12)
```
GET    /settings
PUT    /settings/payment/momo
PUT    /settings/payment/vnpay
PUT    /settings/payment/bank         ← MỚI: STK ngân hàng
PUT    /settings/notification/zalo
PUT    /settings/notification/sms     ← MỚI
PUT    /settings/notification/email   ← MỚI
PUT    /settings/billing
PUT    /settings/company              ← MỚI: thông tin công ty
POST   /settings/payment/momo/test
POST   /settings/payment/vnpay/test
POST   /settings/logo                 ← MỚI: upload logo
```

### AI AGENT (3)
```
POST   /ai/chat
GET    /ai/conversations
DELETE /ai/conversations/{id}
```

### SUBSCRIPTIONS (7) — MỚI
```
GET    /subscriptions/current
GET    /subscriptions/plans
GET    /subscriptions/history
POST   /subscriptions/upgrade
POST   /subscriptions/cancel
GET    /subscriptions/payment/momo    ← tạo link thanh toán gói
GET    /subscriptions/payment/vnpay
```

### ONBOARDING (4) — MỚI
```
GET    /onboarding/status
POST   /onboarding/create-property
POST   /onboarding/add-rooms
POST   /onboarding/complete
```

### FILE UPLOAD (3) — MỚI
```
POST   /files/presign                 ← lấy presigned URL từ R2
POST   /files/upload                  ← upload trực tiếp (fallback)
DELETE /files
```

### SYSTEM (3)
```
GET    /health
GET    /health/detailed               ← DB + Redis status
GET    /metrics                       ← prometheus format (internal)
```

---

## Tổng: ~130 endpoints

| Nhóm | Số endpoints |
|------|-------------|
| Auth | 6 |
| Properties | 8 |
| Rooms | 12 |
| Meter Readings | 5 |
| Customers | 10 |
| Contracts | 11 |
| Co-tenants | 4 |
| Invoices | 13 |
| Payments | 7 |
| Transactions | 6 |
| Reports | 8 |
| Notifications | 4 |
| Staff | 9 |
| Settings | 12 |
| AI Agent | 3 |
| Subscriptions | 7 |
| Onboarding | 4 |
| Files | 3 |
| System | 3 |
| **Tổng** | **135** |
