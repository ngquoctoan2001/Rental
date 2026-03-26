<div align="center">

# 🏠 RentalOS

**Hệ thống quản lý nhà trọ toàn diện — thông minh, đa chi nhánh, tích hợp AI**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Next.js](https://img.shields.io/badge/Next.js-15-000000?style=flat-square&logo=nextdotjs)](https://nextjs.org)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)](https://postgresql.org)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

</div>

---

## ✨ Tính năng nổi bật

| Tính năng | Mô tả |
|-----------|-------|
| 🏢 **Đa chi nhánh** | Mỗi tenant có schema PostgreSQL riêng biệt (row-level isolation) |
| 🤖 **AI Assistant** | Chat với Claude AI — truy vấn dữ liệu, phân tích báo cáo bằng ngôn ngữ tự nhiên |
| 💳 **Thanh toán đa kênh** | Tích hợp MoMo, VNPay, ZaloPay, chuyển khoản ngân hàng |
| 📊 **Báo cáo thời gian thực** | Doanh thu, tỷ lệ lấp đầy, xu hướng quá hạn |
| 🔔 **Thông báo tự động** | Zalo OA, Email, SMS — nhắc hạn hợp đồng, nhắc đóng tiền |
| 📸 **OCR hóa đơn** | Scan ảnh hóa đơn điện nước bằng Google Vision AI |
| ⏱️ **Background Jobs** | Hangfire — tự động đánh dấu quá hạn, gửi báo cáo tháng |
| 🔒 **Bảo mật** | JWT + Refresh Token, Role-based (Owner / Manager / Staff) |

---

## 🏗️ Kiến trúc

```
RentalOS (Clean Architecture)
├── RentalOS.Domain          # Entities, Enums, Domain Events, Exceptions
├── RentalOS.Application     # CQRS (MediatR), Validators, Interfaces
├── RentalOS.Infrastructure  # EF Core, Repositories, External Services
├── RentalOS.API             # ASP.NET Core 10 – REST API + SignalR
├── RentalOS.Worker          # Hangfire Background Job Worker
└── rentalos-web/            # Next.js 15 + TypeScript – Frontend SPA
```

**Stack chính:**

- **Backend:** .NET 10, MediatR, FluentValidation, AutoMapper, Dapper, EF Core, Polly, ClosedXML
- **Frontend:** Next.js 15, TypeScript, Tailwind CSS, Tanstack Query, Zustand, Framer Motion, React Hook Form + Zod
- **Database:** PostgreSQL 16 – multi-tenant schema per tenant
- **Storage:** Cloudflare R2 (S3-compatible)
- **AI:** Anthropic Claude (streaming)
- **Jobs:** Hangfire (PostgreSQL storage)
- **Deploy:** Docker + Railway

---

## 🚀 Chạy dự án local

### Yêu cầu

| Công cụ | Phiên bản | Download |
|---------|-----------|----------|
| .NET SDK | 10.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Node.js | 22.x LTS | [nodejs.org](https://nodejs.org) |
| PostgreSQL | 16+ | [postgresql.org](https://www.postgresql.org/download/) hoặc Docker |
| Docker Desktop | Latest | [docker.com](https://www.docker.com/products/docker-desktop/) *(tuỳ chọn)* |

---

### Bước 1 — Clone repo

```bash
git clone <repo-url>
cd Rental
```

---

### Bước 2 — Khởi động PostgreSQL

> Nếu đã có PostgreSQL chạy local ở port 5432, bỏ qua bước này.

```bash
# Chạy PostgreSQL bằng Docker
docker run -d \
  --name rentalos-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=20102001 \
  -e POSTGRES_DB=rentalos \
  -p 5432:5432 \
  postgres:16
```

---

### Bước 3 — Cấu hình Backend

Tạo file `src/RentalOS.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=rentalos;Username=postgres;Password=20102001"
  },
  "Jwt": {
    "Secret": "SuperSecretKeyForRentalOS_2025_IdentityV3_AdvancedAuth!",
    "Issuer": "RentalOS",
    "Audience": "RentalOS_Users",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },
  "CloudflareR2": {
    "ServiceUrl": "https://<account-id>.r2.cloudflarestorage.com",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "BucketName": "rentalos-files",
    "PublicUrl": "https://files.your-domain.com"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-..."
  }
}
```

> **Lưu ý:** Nếu chưa có Cloudflare R2 hay Anthropic API key, hệ thống vẫn chạy được (các tính năng liên quan sẽ báo lỗi khi dùng).

---

### Bước 4 — Apply Database Migration

```bash
cd src/RentalOS.API
dotnet ef database update
cd ../..
```

---

### Bước 5 — Chạy API

```bash
cd src/RentalOS.API
dotnet run
```

API khởi động tại: **http://localhost:5000**  
Swagger UI: **http://localhost:5000/swagger**

---

### Bước 6 — Chạy Worker (Background Jobs)

Mở terminal mới:

```bash
cd src/RentalOS.Worker
dotnet run
```

Hangfire Dashboard: **http://localhost:5001/hangfire**

---

### Bước 7 — Chạy Frontend

Mở terminal mới:

```bash
cd rentalos-web

# Tạo file môi trường
echo "NEXT_PUBLIC_API_URL=http://localhost:5000/api" > .env.local

# Cài dependencies (lần đầu)
npm install

# Khởi động dev server
npm run dev
```

Frontend: **http://localhost:3000**

---

### Bước 8 — Tạo tài khoản đầu tiên

Gọi API đăng ký (dùng Swagger hoặc curl):

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "owner@demo.vn",
    "password": "Demo@2025!",
    "fullName": "Chủ nhà Demo",
    "tenantName": "Nhà Trọ Demo",
    "tenantSlug": "demo"
  }'
```

Sau đó đăng nhập tại **http://localhost:3000/login**.

---

## 🗄️ Cấu trúc Database

RentalOS dùng **multi-tenant schema isolation** trên PostgreSQL:

- Schema `public` — bảng `tenants`, `users`, `subscriptions`
- Schema `tenant_<slug>` — toàn bộ dữ liệu của từng tenant (phòng, hợp đồng, hóa đơn, ...)

---

## 🐳 Deploy với Docker

```bash
# Build image
docker build -t rentalos-api .

# Chạy
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=<db-host>;..." \
  rentalos-api
```

---

## 🚂 Deploy lên Railway

1. Push code lên GitHub
2. Kết nối repo với [Railway](https://railway.app)
3. Thêm PostgreSQL service từ Railway marketplace
4. Set environment variables trong Railway dashboard (xem [docs/SETUP.md](docs/SETUP.md))
5. Deploy tự động từ `main` branch

```bash
# Hoặc dùng Railway CLI
railway up
```

---

## 📁 Cấu trúc thư mục

```
Rental/
├── src/
│   ├── RentalOS.API/          # REST API Controllers, Middleware, Program.cs
│   ├── RentalOS.Application/  # Commands, Queries, Validators, Interfaces
│   ├── RentalOS.Domain/       # Entities, Enums, Domain Events
│   ├── RentalOS.Infrastructure/ # EF Core, Services (AI, Payment, Storage...)
│   └── RentalOS.Worker/       # Hangfire Jobs
├── rentalos-web/              # Next.js Frontend
│   └── src/
│       ├── app/               # App Router pages
│       ├── components/        # Shared UI components
│       ├── lib/               # API clients, stores, hooks, schemas
│       └── types/             # TypeScript type definitions
├── tests/
│   ├── RentalOS.UnitTests/
│   └── RentalOS.IntegrationTests/
├── docs/                      # Tài liệu kỹ thuật
├── Dockerfile
├── railway.toml
└── RentalOS.slnx
```

---

## 📜 License

MIT © 2025 RentalOS Team
