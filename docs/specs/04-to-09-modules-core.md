# RentalOS — 04. Module Auth

---

## Endpoints

### POST /auth/register
Request:
```json
{
  "ownerName": "Nguyễn Văn A",
  "ownerEmail": "nva@email.com",
  "phone": "0901234567",
  "tenantName": "Nhà trọ Anh Hùng",
  "password": "StrongPass123!"
}
```
Logic:
1. Validate: email format, password >= 8 chars (chữ + số + ký tự đặc biệt), phone VN
2. Kiểm tra email chưa tồn tại trong `public.tenants`
3. Generate slug: lowercase, bỏ dấu tiếng Việt, replace spaces với `-`, thêm 4 số random nếu trùng
4. Hash password: BCrypt cost=12
5. INSERT vào `public.tenants` với `plan=trial`, `trial_ends_at = now() + 14 days`
6. Tạo schema: `CREATE SCHEMA IF NOT EXISTS tenant_{slug}`
7. Chạy DDL: tạo toàn bộ 14 bảng per-tenant
8. INSERT user owner vào `tenant_{slug}.users`
9. INSERT 10 settings mặc định
10. Generate JWT access token + refresh token
11. Lưu refresh token vào Redis: key `refresh:{token}`, value `{userId}:{tenantSlug}`, TTL 30 days
12. Gửi email chào mừng (async, không block response)
13. Return: `{ accessToken, refreshToken, user, tenant }`

Response:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "user": { "id": "uuid", "email": "...", "fullName": "...", "role": "owner" },
    "tenant": { "id": "uuid", "name": "...", "slug": "...", "plan": "trial", "trialEndsAt": "..." }
  }
}
```

### POST /auth/login
Request: `{ "email", "password", "tenantSlug" }`

Logic:
1. Tìm tenant bằng slug trong `public.tenants` → 404 nếu không tồn tại hoặc `is_active=false`
2. `SET search_path TO tenant_{slug}`
3. Tìm user bằng email → 401 nếu không tìm thấy hoặc `is_active=false`
4. BCrypt.Verify(password, passwordHash) → 401 nếu sai
5. Update `last_login_at = NOW()`
6. Generate JWT: claims = `{ sub: userId, tenant_slug: slug, role, email, exp: +1h }`
7. Generate refresh token, lưu Redis TTL 30 days
8. Return auth result

### POST /auth/refresh
- Đọc `refreshToken` từ HttpOnly cookie hoặc body
- Lookup Redis → lấy userId + tenantSlug
- Generate access token mới
- Rotate refresh token (xóa cũ, tạo mới)

### POST /auth/forgot-password
- Nhận email + tenantSlug
- Generate reset token (UUID), lưu Redis TTL 1h: key `reset:{token}` = `{userId}:{tenantSlug}`
- Gửi email với link: `{APP_URL}/reset-password?token={token}`

### POST /auth/reset-password
- Nhận token + newPassword
- Lookup Redis → lấy userId
- BCrypt.Hash(newPassword) → update DB
- Xóa token khỏi Redis

### PUT /auth/change-password [Authorize]
- Nhận currentPassword + newPassword
- Verify current password trước
- Hash và update

---

## JWT Claims
```json
{
  "sub": "user-uuid",
  "tenant_slug": "nha-tro-anh-hung",
  "role": "owner",
  "email": "nva@email.com",
  "plan": "pro",
  "iat": 1700000000,
  "exp": 1700003600
}
```

---

# RentalOS — 05. Module Properties (Nhà trọ)

---

## Endpoints

### GET /properties
Query: `?page&pageSize&search&isActive`
Response: list PropertyDto với `roomSummary: { total, available, rented, maintenance }`

### GET /properties/{id}/stats
```json
{
  "totalRooms": 20,
  "availableRooms": 3,
  "rentedRooms": 16,
  "maintenanceRooms": 1,
  "occupancyRate": 80.0,
  "monthlyRevenue": 48000000,
  "outstandingInvoices": 3,
  "totalOutstandingAmount": 8500000,
  "upcomingContractExpiries": 2,
  "averageRent": 2750000
}
```

### POST /properties
```json
{
  "name": "Nhà trọ Bình Thạnh",
  "address": "123 Đinh Tiên Hoàng, P.3",
  "province": "Hồ Chí Minh",
  "district": "Bình Thạnh",
  "ward": "Phường 3",
  "description": "...",
  "totalFloors": 4
}
```
Validation: name 2-200 chars, address required. Kiểm tra plan limit.

### POST /properties/{id}/images
- Nhận `file` multipart form (jpg/png, max 10MB)
- Upload lên R2: `properties/{id}/{uuid}.jpg`
- Update `properties.cover_image` hoặc append vào `images` array

### DELETE /properties/{id}
- Soft delete: `is_active = false`
- Chỉ được xóa nếu không có phòng đang thuê

---

# RentalOS — 06. Module Rooms (Phòng trọ)

---

## Endpoints

### GET /rooms
Query: `?propertyId&status&minPrice&maxPrice&floor&amenities=wifi,ac&search&page&pageSize`

### GET /rooms/{id}/meter-readings
Query: `?month=2026-03` — lấy chỉ số điện nước của phòng theo tháng

### GET /rooms/{id}/qrcode
Response: PNG image của QR code chứa URL: `{APP_URL}/pay?room={id}`

### POST /rooms
```json
{
  "propertyId": "uuid",
  "roomNumber": "101",
  "floor": 1,
  "areaSqm": 25.0,
  "basePrice": 2500000,
  "electricityPrice": 3500,
  "waterPrice": 15000,
  "serviceFee": 50000,
  "internetFee": 100000,
  "garbageFee": 20000,
  "amenities": ["wifi", "ac", "private_wc"],
  "notes": ""
}
```

### PATCH /rooms/{id}/status
```json
{
  "status": "maintenance",
  "maintenanceNote": "Sửa điện",
  "estimatedDoneDate": "2026-04-01"
}
```

### POST /rooms/bulk-create — MỚI
```json
{
  "propertyId": "uuid",
  "rooms": [
    { "roomNumber": "101", "floor": 1, "basePrice": 2500000 },
    { "roomNumber": "102", "floor": 1, "basePrice": 2500000 }
  ]
}
```
Tạo nhiều phòng cùng lúc, dùng khi onboarding.

## Room Status Flow
```
available ──(CreateContract)──► rented
rented ──(TerminateContract|ContractExpired)──► available
available ──(SetMaintenance)──► maintenance
maintenance ──(SetAvailable)──► available
```

## Amenities hợp lệ
```
wifi, ac, water_heater, fridge, washing_machine, private_wc,
balcony, window, parking_motorbike, parking_car, elevator, security,
internet, cable_tv, kitchen
```

---

## Meter Readings API

### GET /meter-readings
Query: `?roomId=&month=2026-03&propertyId=`

### POST /meter-readings
```json
{
  "roomId": "uuid",
  "readingDate": "2026-03-05",
  "electricityReading": 1250,
  "waterReading": 48,
  "electricityImage": "https://...",
  "waterImage": "https://...",
  "note": "Nhập ngày 5 tháng 3"
}
```
Logic: lưu vào `meter_readings`. Tự động tính toán nếu có invoice pending tháng đó.

---

# RentalOS — 07. Module Customers (Khách thuê)

---

## Endpoints

### GET /customers
Query: `?search&isBlacklisted&hasActiveContract&page&pageSize`

Response CustomerDto:
```json
{
  "id": "uuid",
  "fullName": "Nguyễn Văn An",
  "phone": "0901234567",
  "idCardNumber": "079123456789",
  "activeContract": {
    "contractCode": "HD-2024-001",
    "roomNumber": "101",
    "propertyName": "Nhà trọ Bình Thạnh"
  },
  "isBlacklisted": false,
  "createdAt": "..."
}
```

### GET /customers/search
Query: `?q=nguyen` — tìm theo tên, phone, CCCD. Dùng cho dropdown khi tạo hợp đồng.

### POST /customers/ocr
```
Content-Type: multipart/form-data
file: [image]
side: front | back
```
Logic:
1. Upload ảnh tạm lên R2
2. Gọi Google Vision API `TEXT_DETECTION`
3. Parse kết quả theo format CCCD/CMND VN
4. Return extracted fields + confidence score
5. Không lưu customer — chỉ trả về data để form điền

Response:
```json
{
  "fullName": "NGUYỄN VĂN AN",
  "idCardNumber": "079123456789",
  "dateOfBirth": "1995-05-15",
  "gender": "male",
  "hometown": "Tỉnh Bến Tre",
  "address": "Xã Tân Thạch, Châu Thành, Bến Tre",
  "issueDate": "2021-03-10",
  "expiryDate": "2031-05-15",
  "confidence": 0.94
}
```

### PATCH /customers/{id}/blacklist
```json
{
  "action": "blacklist",  // hoặc "unblacklist"
  "reason": "Không trả tiền 3 tháng, bỏ đi không báo"
}
```
Chỉ Owner được thực hiện.

---

## Co-tenants API

### GET /contracts/{id}/co-tenants
List tất cả người ở trong hợp đồng.

### POST /contracts/{id}/co-tenants
```json
{
  "customerId": "uuid",   // khách đã có trong hệ thống
  // HOẶC tạo mới:
  "fullName": "Trần Thị B",
  "phone": "0912345678",
  "idCardNumber": "079234567890",
  "movedInAt": "2024-04-01"
}
```

### PUT /contracts/{id}/co-tenants/{cotId}
Cập nhật ngày chuyển vào/ra: `{ "movedOutAt": "2024-12-01" }`

---

# RentalOS — 08. Module Contracts (Hợp đồng)

---

## Endpoints

### POST /contracts
```json
{
  "roomId": "uuid",
  "customerId": "uuid",
  "startDate": "2024-04-01",
  "endDate": "2025-04-01",
  "monthlyRent": 2500000,
  "depositMonths": 2,
  "electricityPrice": 3500,
  "waterPrice": 15000,
  "serviceFee": 50000,
  "internetFee": 100000,
  "garbageFee": 20000,
  "billingDate": 5,
  "paymentDueDays": 10,
  "maxOccupants": 2,
  "terms": "Không nuôi thú cưng.",
  "templateId": "default",
  "depositPaid": true,
  "depositPaidAt": "2024-04-01"
}
```

Handler Logic:
1. Validate room `status == available`
2. Validate customer `is_blacklisted == false`
3. Validate không có contract active nào của room này
4. Lock row (SELECT FOR UPDATE) trên rooms table
5. Generate `contract_code`: query max sequence trong năm → format `HD-{YYYY}-{NNN}`
6. Insert contract
7. Update `rooms.status = 'rented'`
8. Calculate `deposit_amount = monthly_rent × deposit_months`
9. Tạo bản ghi co_tenant cho customer chính (is_primary=true)
10. Generate PDF hợp đồng (QuestPDF) — async job
11. Ghi audit log

### PATCH /contracts/{id}/terminate
```json
{
  "terminationDate": "2024-06-30",
  "terminationType": "normal",
  "reason": "Khách chuyển đi",
  "depositRefund": 4500000,
  "depositDeduction": 500000,
  "deductionReason": "Hư điều hoà"
}
```
Logic:
1. Set `status = terminated`, `terminated_at`, `termination_reason`
2. Update `rooms.status = available`
3. Tạo transaction `category = deposit_refund` nếu `depositRefund > 0`
4. Ghi audit log

### POST /contracts/{id}/renew
```json
{
  "newEndDate": "2026-04-01",
  "newMonthlyRent": 2700000
}
```

### GET /contracts/{id}/pdf
- Nếu `pdf_url` đã có → redirect/return URL
- Nếu chưa có → generate on-the-fly, lưu R2, return URL

## Contract Code Format
```
HD-{YYYY}-{NNN}
Sequence reset về 001 đầu mỗi năm (theo năm của start_date)
```

---

# RentalOS — 09. Module Invoices (Hóa đơn)

---

## Endpoints

### POST /invoices
```json
{
  "contractId": "uuid",
  "billingMonth": "2026-03",
  "electricityOld": 1180,
  "electricityNew": 1250,
  "waterOld": 42,
  "waterNew": 48,
  "internetFee": 100000,
  "garbageFee": 20000,
  "otherFees": 0,
  "discount": 0,
  "notes": "",
  "meterImageElectricity": "https://...",
  "meterImageWater": "https://..."
}
```

Logic:
1. Kiểm tra contract active
2. Kiểm tra chưa có invoice cho tháng này (unique constraint)
3. Tính toán:
   - `electricity_amount = (new - old) × price`
   - `water_amount = (new - old) × price`
   - `total = room_rent + electricity + water + service + internet + garbage + other - discount`
4. Generate `invoice_code`: `INV-{YYYY}-{MM}-{NNN}`
5. Generate `payment_link_token`: `Guid.NewGuid().ToString("N")` — 32 chars hex
6. Set `payment_link_expires_at = now() + 30 days`
7. Set `due_date = billing_month + billing_date days`

### POST /invoices/bulk-generate
```json
{
  "propertyId": null,
  "billingMonth": "2026-03",
  "includeRoomIds": null,
  "sendNotification": true,
  "overwriteExisting": false
}
```
Response:
```json
{
  "generated": 18,
  "skipped": 2,
  "errors": 0,
  "details": [
    { "roomNumber": "P.301", "status": "generated", "invoiceCode": "INV-2026-03-019" },
    { "roomNumber": "P.302", "status": "skipped", "reason": "Đã có hóa đơn tháng này" }
  ]
}
```

### POST /invoices/{id}/send
```json
{ "channel": "zalo" }  // zalo | sms | both | email
```
Logic:
1. Lookup invoice + contract + customer
2. Enqueue Hangfire job: `SendInvoiceNotificationJob`
3. Update `sent_at = NOW()`

### GET /invoices/pending-meter — MỚI
Danh sách hóa đơn chưa nhập chỉ số điện nước (cần nhân viên nhập trước khi gửi).

## Invoice Code Format
```
INV-{YYYY}-{MM}-{NNN}
Sequence theo tháng, reset đầu mỗi tháng
```

## Tính toán hóa đơn
```
electricity_amount = (electricity_new - electricity_old) × electricity_price
water_amount       = (water_new - water_old) × water_price
total_amount       = room_rent
                   + electricity_amount
                   + water_amount
                   + service_fee
                   + internet_fee
                   + garbage_fee
                   + other_fees
                   - discount
```
