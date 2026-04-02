# 🏠 RentalOS

**Hệ thống quản lý nhà trọ toàn diện — thông minh, đa chi nhánh, tích hợp AI**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![Next.js](https://img.shields.io/badge/Next.js-15-000000?style=flat-square&logo=nextdotjs)](https://nextjs.org)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)](https://postgresql.org)
[![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

---

## ✨ Tính năng nổi bật

| Tính năng | Mô tả |
|-----------|-------|
| 🏢 **Đa chi nhánh** | Mỗi tenant có schema PostgreSQL riêng biệt (row-level isolation) |
| 🤖 **AI Assistant** | Chat với Claude AI — truy vấn dữ liệu, phân tích báo cáo bằng ngôn ngữ tự nhiên |
| 💳 **Thanh toán đa kênh** | Tích hợp MoMo, VNPay, ZaloPay, chuyển khoản ngân hàng |
| 📊 **Báo cáo thời gian thực** | Doanh thu, tỷ lệ lấp đầy, xu hướng quá hạn |
| 🔔 **Thông báo tự động** | Zalo OA, Email, SMS (SMTP) |
| 📸 **OCR hóa đơn** | Scan ảnh hóa đơn điện nước bằng Google Vision AI |
| ⏱️ **Background Jobs** | Hangfire — tự động đánh dấu quá hạn, gửi báo cáo tháng |
| 🔒 **Bảo mật** | JWT + Refresh Token, Role-based (Owner / Manager / Staff) |
| ☁️ **Lưu trữ** | Cloudflare R2 (S3-compatible) cho hình ảnh và tài liệu |

---

## 🏗️ Kiến trúc

```
RentalOS (Clean Architecture)
├── src/
│   ├── RentalOS.API/          # ASP.NET Core 10 – REST API + SignalR
│   ├── RentalOS.Application/  # CQRS (MediatR), Validators, Interfaces
│   ├── RentalOS.Domain/       # Entities, Enums, Domain Events, Exceptions
│   ├── RentalOS.Infrastructure/ # EF Core, Repositories, External Services (Storage, AI, Pay)
│   ├── RentalOS.Worker/       # Hangfire Background Job Worker
│   └── RentalOS.Utilities/    # Shared helper utilities
└── rentalos-web/            # Next.js 15 + TypeScript – Frontend SPA
```

**Stack chính:**

- **Backend:** .NET 10 (C#), MediatR, FluentValidation, AutoMapper, EF Core 9+
- **Frontend:** Next.js 15, TypeScript, Tailwind CSS, Tanstack Query, Zustand
- **Database:** PostgreSQL 16 (Multi-tenant via Schema)
- **Storage:** Cloudflare R2
- **Jobs:** Hangfire (PostgreSQL storage)
- **Email:** SMTP (Gmail/SendGrid/...)

---

## 🚀 Chạy dự án local

### Yêu cầu hệ thống

| Công cụ | Phiên bản | Link |
|---------|-----------|------|
| .NET SDK | 10.0+ | [Download](https://dotnet.microsoft.com/download) |
| Node.js | 22.x LTS | [Download](https://nodejs.org) |
| PostgreSQL | 16+ | [Download](https://www.postgresql.org/download/) |

---

### Bước 1 — Clone Repo

```bash
git clone <repo-url>
cd Rental
```

### Bước 2 — Cấu hình Backend

Cập nhật file `src/RentalOS.API/appsettings.json` (hoặc tạo `appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=rentalos;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Secret": "MỘT_KEY_BẢO_MẬT_DÀI_ÍT_NHẤT_32_KÝ_TỰ"
  },
  "CloudflareR2": {
    "ServiceUrl": "https://<account-id>.r2.cloudflarestorage.com",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "BucketName": "rentalos-files",
    "PublicUrl": "https://your-public-cdn.com"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

> **Lưu ý:** Hệ thống đã có cơ chế tự động bỏ qua (guard) nếu `ServiceUrl` của R2 chưa được cấu hình đúng, giúp ứng dụng vẫn khởi động được mà không bị crash.

### Bước 3 — Khởi tạo Database

```bash
# Di chuyển vào thư mục API
cd src/RentalOS.API

# Cập nhật DB schema
dotnet ef database update
```

### Bước 4 — Chạy Application

**Chạy API:**
```bash
dotnet run
```
Swagger UI: [http://localhost:5000/swagger](http://localhost:5000/swagger)

**Chạy Frontend (mở terminal mới):**
```bash
cd rentalos-web
npm install
npm run dev
```
Trang chủ: [http://localhost:3000](http://localhost:3000)

---

## 🗄️ Database Design (Multi-tenancy)

RentalOS sử dụng chiến lược **Schema-based Isolation**:
- `public`: Chứa thông tin hệ thống, danh sách tenants, users dùng chung.
- `tenant_<slug>`: Mỗi khách hàng (chủ nhà) có một schema riêng chứa dữ liệu kinh doanh (phòng, khách thuê, hợp đồng).

Cơ chế này đảm bảo:
1. **Bảo mật**: Dữ liệu chủ nhà A không bao giờ lẫn vào chủ nhà B.
2. **Hiệu năng**: Query trên các bảng nhỏ hơn so với dùng chung 1 bảng khổng lồ.
3. **Mở rộng**: Dễ dàng backup/restore dữ liệu cho từng khách hàng cụ thể.

---

## 📜 Giấy phép

Phát triển bởi **Antigravity**. MIT License.
