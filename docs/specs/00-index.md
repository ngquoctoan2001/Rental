# RentalOS v2.0 — Bộ Đặc tả Hoàn chỉnh

> Phiên bản: 2.0.0 — Đầy đủ, sẵn sàng đưa vào AI agents code  
> Cập nhật: 2026-03-23

---

## Cấu trúc file đặc tả

| File | Nội dung | Dùng cho |
|------|----------|----------|
| `00-index.md` | Tổng quan, gap analysis, hướng dẫn dùng | Đọc đầu tiên |
| `01-architecture.md` | Stack, cấu trúc solution, infrastructure, conventions | Scaffold project |
| `02-database.md` | Toàn bộ 16 bảng SQL, indexes, relations, seed data | Tạo migrations |
| `03-api-conventions.md` | Auth, response format, pagination, error codes, middleware | Setup API layer |
| `04-module-auth.md` | Đăng ký, đăng nhập, JWT, refresh, quên mật khẩu | Module Auth |
| `05-module-properties.md` | Nhà trọ CRUD, stats, upload ảnh | Module Properties |
| `06-module-rooms.md` | Phòng CRUD, status flow, meter readings, QR | Module Rooms |
| `07-module-customers.md` | Khách thuê, OCR CCCD, blacklist, co-tenants | Module Customers |
| `08-module-contracts.md` | Hợp đồng, PDF, gia hạn, thanh lý, deposit tracking | Module Contracts |
| `09-module-invoices.md` | Hóa đơn, bulk generate, payment link, PDF export | Module Invoices |
| `10-module-payments.md` | MoMo, VNPay, tiền mặt, webhook, reconcile | Module Payments |
| `11-module-transactions.md` | Giao dịch, export Excel, báo cáo dòng tiền | Module Transactions |
| `12-module-reports.md` | Báo cáo doanh thu, occupancy, collection rate | Module Reports |
| `13-module-notifications.md` | Zalo OA, SMS, Email, in-app, templates | Module Notifications |
| `14-module-staff.md` | Nhân viên, roles, permissions, audit log | Module Staff |
| `15-module-settings.md` | Cài đặt thanh toán, hóa đơn, thông báo, profile | Module Settings |
| `16-module-ai-agent.md` | AI Agent, 15 tools, streaming, confirmation flow | Module AI |
| `17-module-subscription.md` | Plans, billing, enforcement, onboarding | Module Subscription |
| `18-background-jobs.md` | Hangfire jobs, schedules, logic chi tiết | Background Jobs |
| `19-security.md` | JWT, tenant isolation, rate limit, file upload | Security |
| `20-frontend.md` | Next.js pages, components, state, routing | Frontend |

---

## Những gì còn thiếu so với v1.0

### Bảng DB còn thiếu (v1: 12 → v2: 16)
| Bảng mới | Lý do cần |
|----------|-----------|
| `contract_co_tenants` | Nhiều người ở chung 1 phòng, cần lưu từng người |
| `notification_logs` | Log mọi tin nhắn Zalo/SMS/Email để retry và debug |
| `subscription_payments` | Lịch sử thu tiền subscription từ chủ trọ |
| `payment_link_logs` | Log truy cập trang pay/[token] để debug |

### Module hoàn toàn mới (v2)
- **Module Reports** — báo cáo doanh thu, tỷ lệ lấp đầy, collection rate
- **Module Subscription** — billing, plan enforcement, onboarding wizard
- **Co-tenants** — nhiều người ở chung, quản lý từng cá nhân

### API endpoints còn thiếu (~55 endpoints thêm, tổng v2: ~130)
- Reports: 8 endpoints
- Subscription: 7 endpoints
- Co-tenants: 4 endpoints
- Meter readings CRUD riêng: 5 endpoints
- Notification logs: 3 endpoints
- Dashboard summary: 3 endpoints
- File upload presigned URL: 2 endpoints
- Health check + metrics: 3 endpoints
- Onboarding: 4 endpoints
- AI tools mở rộng: 6 tools thêm (v1: 9 → v2: 15)
- Settings mở rộng: 8 endpoints thêm

### Logic còn thiếu
- File upload flow (presigned URL qua Cloudflare R2)
- Realtime notifications (SignalR hub)
- Onboarding wizard cho tenant mới
- PDF generation (hợp đồng + hóa đơn)
- Excel export
- Subscription auto-renew flow
- Plan enforcement chi tiết từng feature
- Co-tenant management
- Deposit tracking (hoàn cọc khi thanh lý)
- Meter reading history + trend

---

## Hướng dẫn đưa vào AI agents

### Dùng với GitHub Copilot (VS Code)
```
Đọc file docs/specs/{tên-file}.md và implement đúng theo đặc tả.
Không thêm, không bớt logic so với đặc tả.
```

### Dùng với Claude / ChatGPT
Paste nội dung file liên quan vào context trước khi yêu cầu code.
Ví dụ: implement module Contracts → paste file `08-module-contracts.md`.

### Thứ tự implement
```
01 → 02 → 03 → 04 → 05 → 06 → 07 → 08 → 09 → 10
→ 11 → 12 → 13 → 14 → 15 → 16 → 17 → 18 → 19 → 20
```
