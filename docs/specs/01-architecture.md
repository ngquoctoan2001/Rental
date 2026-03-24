# RentalOS — 01. Kiến trúc Kỹ thuật

---

## Stack

```
Backend:    ASP.NET 10, C# 13, Minimal API + Controllers
Pattern:    Clean Architecture, CQRS (MediatR), Repository Pattern
ORM:        Entity Framework Core 9 + Dapper (báo cáo phức tạp)
Validation: FluentValidation
Queue:      Hangfire + PostgreSQL storage
Cache:      Redis (session, rate limit, idempotency, pub/sub)
Realtime:   SignalR (in-app notifications)
Storage:    Cloudflare R2 (ảnh, PDF, file hợp đồng)
PDF:        QuestPDF (generate hóa đơn, hợp đồng)
Excel:      ClosedXML (export báo cáo)

Frontend:   Next.js 15 (App Router), TypeScript strict
UI:         TailwindCSS + Shadcn/UI
State:      Zustand (global) + TanStack Query v5 (server state)
Form:       React Hook Form + Zod
Charts:     Recharts

Database:   PostgreSQL 16 (Railway) — schema-per-tenant
Infra:      Railway (API + Worker), Vercel (Frontend), Cloudflare (DNS/SSL/CDN/R2)
```

---

## Cấu trúc Solution ASP.NET

```
RentalOS.sln
├── src/
│   ├── RentalOS.API/
│   │   ├── Controllers/
│   │   ├── Middleware/
│   │   │   ├── TenantMiddleware.cs
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   └── RateLimitMiddleware.cs
│   │   ├── Hubs/
│   │   │   └── NotificationHub.cs      ← SignalR
│   │   └── Program.cs
│   │
│   ├── RentalOS.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs            ← Result<T> pattern
│   │   │   │   ├── PagedResult.cs
│   │   │   │   └── ApiResponse.cs
│   │   │   └── Behaviors/
│   │   │       ├── ValidationBehavior.cs
│   │   │       ├── LoggingBehavior.cs
│   │   │       └── AuditBehavior.cs
│   │   └── Modules/
│   │       ├── Auth/
│   │       ├── Properties/
│   │       ├── Rooms/
│   │       ├── Customers/
│   │       ├── Contracts/
│   │       ├── Invoices/
│   │       ├── Payments/
│   │       ├── Transactions/
│   │       ├── Reports/
│   │       ├── Notifications/
│   │       ├── Staff/
│   │       ├── Settings/
│   │       ├── AI/
│   │       └── Subscriptions/
│   │
│   ├── RentalOS.Domain/
│   │   ├── Entities/
│   │   ├── Enums/
│   │   ├── Events/                      ← Domain Events
│   │   └── Exceptions/
│   │
│   ├── RentalOS.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/          ← EF Fluent API
│   │   │   ├── Migrations/
│   │   │   └── Interceptors/
│   │   │       └── AuditInterceptor.cs
│   │   ├── Multitenancy/
│   │   │   ├── TenantContext.cs
│   │   │   └── TenantSchemaManager.cs
│   │   ├── Repositories/
│   │   ├── Services/
│   │   │   ├── Payments/
│   │   │   │   ├── MoMoService.cs
│   │   │   │   └── VNPayService.cs
│   │   │   ├── Notifications/
│   │   │   │   ├── ZaloService.cs
│   │   │   │   ├── SmsService.cs
│   │   │   │   └── EmailService.cs
│   │   │   ├── Storage/
│   │   │   │   └── R2StorageService.cs
│   │   │   ├── AI/
│   │   │   │   └── AnthropicService.cs
│   │   │   ├── Pdf/
│   │   │   │   └── PdfService.cs        ← QuestPDF
│   │   │   └── Excel/
│   │   │       └── ExcelService.cs      ← ClosedXML
│   │   └── BackgroundJobs/
│   │
│   └── RentalOS.Worker/
│       └── Jobs/
│
└── tests/
    ├── RentalOS.UnitTests/
    └── RentalOS.IntegrationTests/
```

---

## Cấu trúc Next.js

```
rentalos-web/
├── src/
│   ├── app/
│   │   ├── (auth)/
│   │   │   ├── login/page.tsx
│   │   │   ├── register/page.tsx
│   │   │   ├── forgot-password/page.tsx
│   │   │   └── accept-invite/page.tsx    ← Nhân viên nhận lời mời
│   │   ├── (dashboard)/
│   │   │   ├── layout.tsx               ← Sidebar + Header
│   │   │   ├── page.tsx                 ← Dashboard
│   │   │   ├── properties/
│   │   │   ├── rooms/
│   │   │   ├── customers/
│   │   │   ├── contracts/
│   │   │   ├── invoices/
│   │   │   ├── transactions/
│   │   │   ├── reports/                 ← MỚI
│   │   │   ├── ai-assistant/
│   │   │   ├── staff/
│   │   │   └── settings/
│   │   ├── onboarding/                  ← MỚI — wizard cho tenant mới
│   │   │   ├── welcome/page.tsx
│   │   │   ├── create-property/page.tsx
│   │   │   ├── add-rooms/page.tsx
│   │   │   └── complete/page.tsx
│   │   ├── pay/[token]/page.tsx         ← Public payment page
│   │   └── subscribe/page.tsx           ← MỚI — trang mua gói
│   │
│   ├── components/
│   │   ├── layout/
│   │   │   ├── Sidebar.tsx
│   │   │   ├── Header.tsx
│   │   │   └── NotificationDropdown.tsx ← SignalR realtime
│   │   ├── ui/                          ← Shadcn components
│   │   └── shared/
│   │       ├── DataTable.tsx
│   │       ├── StatusBadge.tsx
│   │       ├── StatCard.tsx
│   │       ├── ConfirmDialog.tsx
│   │       ├── FileUpload.tsx
│   │       └── PlanLimitBanner.tsx      ← Hiện khi gần đạt giới hạn
│   │
│   ├── lib/
│   │   ├── api/
│   │   │   ├── client.ts                ← Axios + interceptors
│   │   │   ├── auth.ts
│   │   │   ├── properties.ts
│   │   │   ├── rooms.ts
│   │   │   ├── customers.ts
│   │   │   ├── contracts.ts
│   │   │   ├── invoices.ts
│   │   │   ├── payments.ts
│   │   │   ├── transactions.ts
│   │   │   ├── reports.ts
│   │   │   ├── notifications.ts
│   │   │   ├── staff.ts
│   │   │   ├── settings.ts
│   │   │   ├── ai.ts
│   │   │   └── subscriptions.ts
│   │   ├── hooks/
│   │   │   ├── useNotifications.ts      ← SignalR hook
│   │   │   └── usePlanLimit.ts
│   │   └── stores/
│   │       ├── authStore.ts
│   │       ├── uiStore.ts
│   │       └── notificationStore.ts
│   │
│   └── middleware.ts                    ← Auth guard
```

---

## Coding Conventions

### C# — Result Pattern
```csharp
// Luôn dùng Result<T>, không throw exception trong business logic
public record Result<T>(bool IsSuccess, T? Data, string? ErrorCode, string? ErrorMessage)
{
    public static Result<T> Ok(T data) => new(true, data, null, null);
    public static Result<T> Fail(string code, string message) => new(false, default, code, message);
}
```

### C# — Module folder structure
```
Modules/Rooms/
  Commands/
    CreateRoom/
      CreateRoomCommand.cs          ← record với properties
      CreateRoomCommandHandler.cs   ← IRequestHandler<>
      CreateRoomCommandValidator.cs ← AbstractValidator<>
  Queries/
    GetRooms/
      GetRoomsQuery.cs
      GetRoomsQueryHandler.cs
      RoomDto.cs                    ← output DTO
  Events/
    RoomCreatedEvent.cs             ← Domain event (optional)
```

### TypeScript — API types
```typescript
// Tất cả API types trong src/types/{module}.ts
export interface Room {
  id: string
  propertyId: string
  roomNumber: string
  status: 'available' | 'rented' | 'maintenance'
  basePrice: number
  // ...
}

export interface ApiResponse<T> {
  success: boolean
  data: T
  meta?: PaginationMeta
}
```

### Environment Variables — Đầy đủ
```env
# === BACKEND (ASP.NET) ===

# Database
DATABASE_URL=postgresql://user:pass@host:5432/rentalos

# JWT
JWT_SECRET_KEY=min-32-chars-secret-key
JWT_ISSUER=rentalos.vn
JWT_AUDIENCE=rentalos-users
JWT_ACCESS_TOKEN_EXPIRE_MINUTES=60
JWT_REFRESH_TOKEN_EXPIRE_DAYS=30

# Redis
REDIS_URL=redis://localhost:6379
REDIS_PASSWORD=

# Cloudflare R2
R2_ACCOUNT_ID=
R2_ACCESS_KEY_ID=
R2_SECRET_ACCESS_KEY=
R2_BUCKET_NAME=rentalos-files
R2_PUBLIC_URL=https://files.rentalos.vn

# MoMo
MOMO_PARTNER_CODE=
MOMO_ACCESS_KEY=
MOMO_SECRET_KEY=
MOMO_ENDPOINT=https://payment.momo.vn
MOMO_SANDBOX_ENDPOINT=https://test-payment.momo.vn

# VNPay
VNPAY_TMN_CODE=
VNPAY_HASH_SECRET=
VNPAY_URL=https://pay.vnpay.vn/vpcpay.html
VNPAY_SANDBOX_URL=https://sandbox.vnpayment.vn/paymentv2/vpcpay.html

# AI
ANTHROPIC_API_KEY=
ANTHROPIC_MODEL=claude-sonnet-4-20250514

# Zalo OA
ZALO_OA_ACCESS_TOKEN=
ZALO_OA_REFRESH_TOKEN=
ZALO_OA_APP_ID=
ZALO_OA_APP_SECRET=

# SMS (VNPT)
VNPT_SMS_USERNAME=
VNPT_SMS_PASSWORD=
VNPT_SMS_BRANDNAME=RentalOS

# Email (SendGrid)
SENDGRID_API_KEY=
EMAIL_FROM=noreply@rentalos.vn
EMAIL_FROM_NAME=RentalOS

# Google Vision (OCR)
GOOGLE_VISION_API_KEY=

# App
APP_URL=https://rentalos.vn
API_URL=https://api.rentalos.vn
ENVIRONMENT=production  # development | staging | production

# === FRONTEND (Next.js) ===
NEXT_PUBLIC_API_URL=https://api.rentalos.vn/api/v1
NEXT_PUBLIC_APP_URL=https://rentalos.vn
NEXT_PUBLIC_SIGNALR_URL=https://api.rentalos.vn/hubs
```
