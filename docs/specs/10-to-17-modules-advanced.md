# RentalOS — 10. Module Payments (Thanh toán)

---

## MoMo Integration

### POST /payments/momo/create [Authorize]
```json
{
  "invoiceId": "uuid",
  "returnUrl": "https://rentalos.vn/pay/success",
  "notifyUrl": "https://api.rentalos.vn/api/v1/payments/momo/webhook"
}
```
Logic:
1. Lookup invoice + tenant MoMo config
2. Build MoMo request: `orderId = {tenantSlug}-{invoiceCode}-{timestamp}`
3. Sign: `HMAC-SHA256(rawSignString, secretKey)`
4. POST tới MoMo API v2
5. Log request vào `payment_link_logs`
6. Return `{ payUrl, qrCodeUrl, deeplink }`

### POST /payments/momo/webhook [public]
Logic:
1. Raw body HMAC-SHA256 verify với secretKey của tenant
   - Tenant xác định bằng `orderId` prefix (tenantSlug)
2. Redis idempotency check: `payment:idempotent:momo:{orderId}` → return 200 nếu đã xử lý
3. Parse `resultCode` — chỉ xử lý nếu `resultCode == 0` (success)
4. Lookup invoice bằng `orderId`
5. Tạo transaction record
6. Update `invoice.status = paid`, `invoice.paid_at = NOW()`
7. Redis set idempotency key, TTL 72h
8. Enqueue job: `SendPaymentConfirmationJob`
9. Broadcast SignalR event tới dashboard chủ trọ

---

## VNPay Integration

### POST /payments/vnpay/create [Authorize]
Logic: Build query string theo VNPay spec, sign HMAC-SHA512, return `paymentUrl`

### POST /payments/vnpay/webhook [public]
Verify `vnp_SecureHash` → xử lý tương tự MoMo

### GET /payments/vnpay/return [public]
Trang redirect sau khi khách thanh toán. Hiển thị success/fail page.

---

## Trang thanh toán công khai GET /pay/{token}

Response: invoice detail cho public consumption
```json
{
  "invoiceCode": "INV-2026-03-001",
  "billingMonth": "2026-03",
  "dueDate": "2026-03-15",
  "propertyName": "Nhà trọ Bình Thạnh",
  "roomNumber": "101",
  "customerName": "Nguyễn Văn An",
  "breakdown": {
    "roomRent": 2500000,
    "electricityAmount": 245000,
    "electricityUsed": 70,
    "waterAmount": 90000,
    "waterUsed": 6,
    "serviceFee": 50000,
    "internetFee": 100000,
    "totalAmount": 2985000
  },
  "status": "pending",
  "isExpired": false,
  "availablePaymentMethods": ["momo", "vnpay", "bank_transfer"],
  "bankInfo": {
    "bankName": "MB Bank",
    "accountNumber": "1234567890",
    "accountName": "NGUYEN VAN HUNG",
    "transferContent": "INV-2026-03-001"
  }
}
```
Ghi log vào `payment_link_logs` mỗi lần truy cập.

---

## POST /transactions/deposit-refund — MỚI

```json
{
  "contractId": "uuid",
  "amount": 4500000,
  "note": "Hoàn cọc khi thanh lý HD-2024-001",
  "method": "cash",
  "paidAt": "2026-03-20T10:00:00Z"
}
```
Tạo transaction `direction=expense, category=deposit_refund`.

---

# RentalOS — 11. Module Transactions (Giao dịch)

---

## GET /transactions
Query: `?invoiceId&method&direction&category&dateFrom&dateTo&propertyId&page&pageSize`

## GET /transactions/summary
```json
{
  "period": { "from": "2026-03-01", "to": "2026-03-31" },
  "totalIncome": 86400000,
  "totalExpense": 4500000,
  "netCashflow": 81900000,
  "byMethod": {
    "momo": 45000000,
    "vnpay": 32400000,
    "cash": 9000000
  },
  "byCategory": {
    "rent": 86400000,
    "deposit_refund": 4500000
  },
  "collectionRate": 87.5,
  "totalInvoiced": 98700000,
  "totalCollected": 86400000,
  "totalOutstanding": 12300000
}
```

## GET /transactions/export
Query: `?dateFrom&dateTo&format=xlsx`
Response: Excel file với tất cả giao dịch trong kỳ.

---

# RentalOS — 12. Module Reports (Báo cáo) — MỚI

---

## GET /reports/dashboard
Dữ liệu cho dashboard trang tổng quan.
```json
{
  "rooms": {
    "total": 48,
    "available": 6,
    "rented": 40,
    "maintenance": 2,
    "occupancyRate": 83.3
  },
  "revenue": {
    "thisMonth": 86400000,
    "lastMonth": 77000000,
    "changePercent": 12.2
  },
  "invoices": {
    "pendingCount": 8,
    "overdueCount": 3,
    "pendingAmount": 23600000,
    "overdueAmount": 8850000
  },
  "contracts": {
    "expiringIn30Days": 3,
    "expiringIn7Days": 1
  },
  "alerts": [
    { "type": "contract_expiry", "count": 3, "message": "3 hợp đồng hết hạn trong 30 ngày" },
    { "type": "overdue_invoice", "count": 3, "message": "3 hóa đơn quá hạn chưa thu" }
  ]
}
```

## GET /reports/revenue
Query: `?period=this_month|last_month|this_quarter|this_year|custom&from&to&propertyId&groupBy=month|property|method`

Response:
```json
{
  "period": { "from": "...", "to": "..." },
  "summary": {
    "totalRevenue": 86400000,
    "collectionRate": 87.5,
    "avgMonthlyRevenue": 82000000
  },
  "byMonth": [
    { "month": "2026-03", "invoiced": 98700000, "collected": 86400000, "rate": 87.5 }
  ],
  "byProperty": [
    { "propertyName": "Nhà trọ BT", "invoiced": 50000000, "collected": 45000000, "rate": 90 }
  ],
  "byMethod": {
    "momo": 45000000, "vnpay": 32400000, "cash": 9000000
  }
}
```

## GET /reports/occupancy
Tỷ lệ lấp đầy theo thời gian và theo nhà trọ.
```json
{
  "current": { "occupied": 40, "total": 48, "rate": 83.3 },
  "history": [
    { "month": "2026-03", "rate": 83.3 },
    { "month": "2026-02", "rate": 81.2 }
  ],
  "byProperty": [
    { "propertyName": "Nhà trọ BT", "occupied": 18, "total": 20, "rate": 90 }
  ]
}
```

## GET /reports/collection-rate
Tỷ lệ thu tiền theo tháng, phòng, khu vực.

## GET /reports/export
Query: `?type=revenue|occupancy|transactions&period=...&format=xlsx`
Response: Excel file.

---

# RentalOS — 13. Module Notifications (Thông báo)

---

## Kênh thông báo

| Kênh | Dùng khi | Provider |
|------|----------|----------|
| Zalo OA | Tin nhắn hóa đơn, nhắc nợ, xác nhận | Zalo API v3 |
| SMS | Khi không có Zalo | VNPT SMS |
| Email | Hợp đồng PDF, báo cáo tháng, mời nhân viên | SendGrid |
| In-app | Realtime dashboard | SignalR |

---

## Notification Events & Templates

### invoice_created
- Kênh: Zalo (primary) + SMS (fallback nếu không có Zalo)
- Người nhận: khách thuê (tất cả co-tenants)
- Template Zalo:
```
🏠 HÓA ĐƠN THÁNG {month}
━━━━━━━━━━━━━━━━
Phòng: {room_number} - {property_name}
Tiền phòng:  {room_rent}đ
Tiền điện:   {elec_amount}đ ({elec_used} kWh)
Tiền nước:   {water_amount}đ ({water_used} m³)
Phí DV:      {service_fee}đ
━━━━━━━━━━━━━━━━
TỔNG CỘNG: {total_amount}đ
Hạn TT: {due_date}

💳 Thanh toán: {payment_link}
```

### invoice_reminder (sắp đến hạn)
- Trigger: `due_date - today IN [3, 1]`
- Template: "Nhắc nhở: Hóa đơn {invoice_code} ({total_amount}đ) đến hạn {due_date}. Link: {payment_link}"

### invoice_overdue (quá hạn)
- Trigger: `due_date < today AND status = pending/overdue`, check sau D+1, D+3, D+7
- Template: "⚠ Hóa đơn {invoice_code} đã quá hạn {days_overdue} ngày. Vui lòng thanh toán: {payment_link}"

### payment_confirmed
- Kênh: Zalo
- Template: "✅ Đã nhận thanh toán {amount}đ cho hóa đơn {invoice_code}. Cảm ơn bạn!"

### contract_expiry_alert
- Trigger: `end_date - today IN [30, 15, 7]`
- Kênh: Email + In-app (chủ trọ), Zalo (khách thuê)

### monthly_report
- Trigger: ngày 1 hàng tháng
- Kênh: Email
- Đính kèm: báo cáo tháng trước dạng PDF

---

## GET /notifications
In-app notifications list cho user hiện tại.
```json
{
  "items": [
    { "id": "uuid", "type": "payment_received", "title": "Thanh toán nhận được",
      "message": "Phòng 101 đã thanh toán 2.985.000đ", "isRead": false, "createdAt": "..." }
  ],
  "unreadCount": 5
}
```

## SignalR Hub: /hubs/notifications
Events:
- `NewNotification` → { id, type, title, message, createdAt }
- `InvoicePaid` → { invoiceId, amount, roomNumber }
- `SystemAlert` → { type, message }

Client connect:
```typescript
const connection = new HubConnectionBuilder()
  .withUrl(`${SIGNALR_URL}?access_token=${token}`)
  .build()

connection.on('NewNotification', (notification) => { ... })
await connection.start()
```

---

## GET /notifications/logs — MỚI
Lịch sử gửi Zalo/SMS/Email, dùng để debug.
Query: `?channel&status&entityType&entityId&dateFrom&dateTo`

---

# RentalOS — 14. Module Staff (Nhân viên)

---

## Role Permissions

| Action | Owner | Manager | Staff |
|--------|-------|---------|-------|
| Xem nhà trọ (assigned) | ✓ | ✓ | ✓ |
| Thêm/sửa/xóa nhà trọ | ✓ | ✓ | ✗ |
| Thêm/sửa phòng | ✓ | ✓ | ✗ |
| Thêm khách thuê | ✓ | ✓ | ✓ |
| Tạo/sửa hợp đồng | ✓ | ✓ | ✗ |
| Nhập chỉ số điện nước | ✓ | ✓ | ✓ |
| Tạo hóa đơn (không gửi) | ✓ | ✓ | ✓ |
| Gửi hóa đơn | ✓ | ✓ | ✗ |
| Thu tiền mặt | ✓ | ✓ | ✓ |
| Xem báo cáo | ✓ | ✓ | ✗ |
| Quản lý nhân viên | ✓ | ✗ | ✗ |
| Cài đặt hệ thống | ✓ | ✗ | ✗ |
| AI Agent | ✓ | ✓ | ✗ |
| Xem audit log | ✓ | ✗ | ✗ |

---

## POST /staff (Mời nhân viên)
```json
{
  "email": "staff@email.com",
  "role": "staff",
  "assignedPropertyIds": ["uuid1", "uuid2"]
}
```
Logic:
1. Kiểm tra email chưa tồn tại trong tenant
2. Kiểm tra plan limit (Starter: 2, Pro: 5, Business: unlimited)
3. Tạo user với `is_active=false`, `invite_token`, `invite_expires_at = now() + 48h`
4. Gửi email mời: `{APP_URL}/accept-invite?token={invite_token}&tenant={slug}`

## POST /staff/accept-invite
```json
{
  "token": "invite-token-here",
  "password": "NewPass123!",
  "fullName": "Trần Văn Lâm",
  "phone": "0912345678"
}
```
Logic: validate token, hash password, set `is_active=true`, xóa invite token.

## PATCH /staff/{id}/permissions
```json
{
  "assignedPropertyIds": ["uuid1"]
}
```

---

# RentalOS — 15. Module Settings (Cài đặt)

---

## PUT /settings/payment/bank — MỚI
```json
{
  "bankName": "MB Bank",
  "accountNumber": "1234567890",
  "accountName": "NGUYEN VAN HUNG",
  "branch": "Chi nhánh HCM",
  "isActive": true
}
```

## PUT /settings/company — MỚI
```json
{
  "companyName": "Nhà trọ Anh Hùng",
  "address": "123 Đinh Tiên Hoàng, Q.Bình Thạnh",
  "phone": "0901234567",
  "email": "contact@nhatroanhhung.vn",
  "taxCode": "0123456789",
  "logoUrl": "https://..."
}
```
Dùng trong PDF hóa đơn, hợp đồng.

## PUT /settings/notification/sms — MỚI
```json
{
  "provider": "vnpt",
  "username": "...",
  "password": "...",
  "brandname": "RentalOS",
  "isActive": true
}
```

## PUT /settings/notification/email — MỚI
```json
{
  "provider": "sendgrid",
  "apiKey": "SG.xxx",
  "fromEmail": "noreply@rentalos.vn",
  "fromName": "RentalOS",
  "isActive": true
}
```

## PUT /settings/billing
```json
{
  "autoGenerateDay": 5,
  "paymentDueDays": 10,
  "remindBeforeDueDays": [3, 1],
  "remindOverdueDays": [1, 3, 7],
  "defaultElectricityPrice": 3500,
  "defaultWaterPrice": 15000,
  "defaultServiceFee": 50000,
  "defaultInternetFee": 100000,
  "defaultGarbageFee": 20000,
  "autoSendNotification": true
}
```

---

# RentalOS — 16. Module AI Agent

---

## POST /ai/chat
```json
{ "conversationId": "uuid-or-null", "message": "Phòng nào đang trống?" }
```
Response: Server-Sent Events (SSE) stream
```
data: {"type":"text","content":"Hiện tại có "}
data: {"type":"text","content":"3 phòng trống:\n"}
data: {"type":"tool_start","tool":"room_list","input":{"status":"available"}}
data: {"type":"tool_end","tool":"room_list","output":{"rooms":[...]}}
data: {"type":"text","content":"• Phòng 101..."}
data: {"type":"done","conversationId":"uuid"}
```

---

## 15 AI Tools (v2 — mở rộng từ 9 lên 15)

### Tools đọc dữ liệu
```json
1. room_list           - Lấy danh sách phòng với filter
2. room_status_overview - Tổng quan lấp đầy theo nhà trọ
3. customer_search     - Tìm kiếm khách thuê
4. contract_list       - Danh sách hợp đồng (filter: active, expiring)
5. contract_expiry_list - Hợp đồng sắp hết hạn
6. invoice_list        - Danh sách hóa đơn (filter: overdue, pending)
7. revenue_report      - Báo cáo doanh thu
8. pending_meter_list  - Phòng chưa nhập chỉ số điện nước  ← MỚI
9. notification_status - Kiểm tra trạng thái gửi tin nhắn ← MỚI
```

### Tools ghi dữ liệu (yêu cầu xác nhận)
```json
10. room_create        - Tạo phòng mới
11. customer_create    - Thêm khách thuê
12. contract_create    - Tạo hợp đồng
13. invoice_generate   - Tạo hóa đơn (1 hoặc hàng loạt)
14. invoice_send       - Gửi link thanh toán
15. meter_reading_save - Lưu chỉ số điện nước       ← MỚI
```

### Tool schema mẫu — meter_reading_save (MỚI)
```json
{
  "name": "meter_reading_save",
  "description": "Lưu chỉ số điện nước cho phòng",
  "parameters": {
    "type": "object",
    "required": ["roomIdentifier", "electricityReading", "waterReading"],
    "properties": {
      "roomIdentifier": { "type": "string", "description": "Số phòng, VD: P.101 hoặc 101" },
      "propertyName": { "type": "string" },
      "electricityReading": { "type": "integer", "description": "Chỉ số công tơ điện hiện tại" },
      "waterReading": { "type": "integer", "description": "Chỉ số đồng hồ nước hiện tại" },
      "readingDate": { "type": "string", "format": "date", "description": "Ngày đọc, mặc định hôm nay" }
    }
  }
}
```

---

## System Prompt
```
Bạn là trợ lý quản lý nhà trọ thông minh của {tenant_name}.
Trả lời bằng tiếng Việt, ngắn gọn, chính xác, thân thiện.

Quy tắc:
- Hành động ĐỌC: thực hiện ngay, không cần xác nhận.
- Hành động GHI (tạo/sửa/xóa): LUÔN tóm tắt chi tiết và hỏi xác nhận trước.
- Nếu thông tin không đủ: hỏi lại, không đoán mò.
- Định dạng tiền: 2.500.000đ (có dấu phân cách nghìn).
- Định dạng ngày: dd/MM/yyyy.

Thông tin hệ thống:
- Ngày hôm nay: {current_date}
- Tenant: {tenant_name}
- Người dùng: {user_name} ({user_role})
```

---

# RentalOS — 17. Module Subscription (Gói dịch vụ) — MỚI

---

## Plans

| | Trial | Starter | Pro | Business |
|---|---|---|---|---|
| Thời gian | 14 ngày | - | - | - |
| Nhà trọ | 1 | 1 | 3 | Không giới hạn |
| Phòng | 20 | 20 | 100 | Không giới hạn |
| Nhân viên | 2 | 2 | 5 | Không giới hạn |
| AI Agent | ✓ | ✗ | ✓ | ✓ |
| Zalo OA | ✓ | ✗ | ✓ | ✓ |
| Báo cáo nâng cao | ✓ | ✗ | ✓ | ✓ |
| Export Excel | ✓ | ✗ | ✓ | ✓ |
| API Public | ✗ | ✗ | ✗ | ✓ |
| Giá (VNĐ/tháng) | Free | 99.000 | 299.000 | 799.000 |

---

## GET /subscriptions/current
```json
{
  "plan": "pro",
  "expiresAt": "2026-04-17",
  "daysLeft": 25,
  "usage": {
    "properties": { "used": 2, "limit": 3 },
    "rooms": { "used": 42, "limit": 100 },
    "staff": { "used": 3, "limit": 5 }
  },
  "features": {
    "aiAgent": true,
    "zaloOA": true,
    "advancedReports": true,
    "excelExport": true
  }
}
```

## GET /subscriptions/plans
Trả về danh sách plans, features, pricing.

## POST /subscriptions/upgrade
```json
{ "plan": "pro", "paymentMethod": "momo" }
```
Logic:
1. Tạo `subscription_payments` record (status=pending)
2. Gọi MoMo để tạo payment URL
3. Return `{ paymentUrl, orderId }`

Sau khi MoMo webhook confirm:
1. Update `subscription_payments.status = success`
2. Update `public.tenants.plan = pro`
3. Update `public.tenants.plan_expires_at = now() + 30 days`
4. Gửi email xác nhận nâng cấp

## POST /subscriptions/cancel
Hủy tự động gia hạn, plan vẫn chạy đến hết kỳ.

---

## Plan Enforcement

Middleware kiểm tra trước mỗi CUD operation:

```csharp
// Ví dụ: tạo phòng mới
var currentRoomCount = await _roomRepo.CountAsync();
var planLimit = _planService.GetRoomLimit(tenant.Plan);
if (currentRoomCount >= planLimit)
    throw new PlanLimitException("TENANT_PLAN_LIMIT",
        $"Bạn đã đạt giới hạn {planLimit} phòng. Nâng cấp plan Pro để tiếp tục.");
```

Feature gate:
```csharp
// Kiểm tra tính năng
if (!_planService.HasFeature(tenant.Plan, Feature.AiAgent))
    return Forbid("AI Agent chỉ có ở gói Pro và Business.");
```

---

## Onboarding Wizard — MỚI

Khi tenant đăng ký xong và `onboarding_done = false`, redirect sang `/onboarding`.

### GET /onboarding/status
```json
{
  "steps": [
    { "key": "company_profile", "title": "Thông tin công ty", "done": false },
    { "key": "create_property",  "title": "Tạo nhà trọ đầu tiên", "done": false },
    { "key": "add_rooms",        "title": "Thêm phòng trọ", "done": false },
    { "key": "payment_setup",    "title": "Cài đặt thanh toán", "done": false }
  ],
  "completedCount": 0,
  "totalCount": 4
}
```

### POST /onboarding/complete
Đánh dấu `tenants.onboarding_done = true`. Redirect về dashboard.
