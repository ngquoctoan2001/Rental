# RentalOS – Local Development Setup

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | Backend API & Worker |
| [Node.js](https://nodejs.org) | 22.x LTS | Frontend (Next.js) |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Latest | PostgreSQL + Hangfire DB |
| [Git](https://git-scm.com) | Latest | Version control |

---

## 1. Clone & Navigate

```bash
git clone <repo-url>
cd Rental
```

---

## 2. Start Infrastructure (Docker)

```bash
docker compose up -d
```

This starts:
- **PostgreSQL** on port `5432` (database: `rentalos`, user: `postgres`, password: `20102001`)

---

## 3. Configure Backend

### API (`src/RentalOS.API/appsettings.Development.json`)

Copy the sample and fill in your values:

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
  },
  "Zalo": {
    "AppId": "your-zalo-app-id",
    "SecretKey": "your-zalo-secret"
  },
  "MoMo": {
    "PartnerCode": "your-momo-partner-code",
    "AccessKey": "your-momo-access-key",
    "SecretKey": "your-momo-secret-key",
    "Endpoint": "https://test-payment.momo.vn/v2/gateway/api/create"
  },
  "VNPay": {
    "TmnCode": "your-vnpay-tmn-code",
    "HashSecret": "your-vnpay-hash-secret",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
  }
}
```

---

## 4. Apply Database Migrations

```bash
cd src/RentalOS.API
dotnet ef database update
cd ../..
```

---

## 5. Run Backend

### Option A – Run Both in Separate Terminals

**Terminal 1 – API:**
```bash
cd src/RentalOS.API
dotnet run
# Listening on: http://localhost:5000
```

**Terminal 2 – Background Worker (Hangfire jobs):**
```bash
cd src/RentalOS.Worker
dotnet run
# Hangfire Dashboard: http://localhost:5001/hangfire
```

### Option B – Run the Whole Solution

```bash
dotnet build RentalOS.slnx
dotnet run --project src/RentalOS.API
```

---

## 6. Configure & Run Frontend

```bash
cd rentalos-web
```

Create `.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

Install dependencies and start:

```bash
npm install
npm run dev
# Frontend: http://localhost:3000
```

---

## 7. Production Build Check

```bash
# Backend
dotnet build RentalOS.slnx

# Frontend
cd rentalos-web
npm run build
```

---

## 8. Test Key Endpoints

Using the included `src/RentalOS.API/RentalOS.API.http` file (VS Code REST Client):

```http
### Register a new tenant
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "owner@demo.vn",
  "password": "Demo@123",
  "fullName": "Demo Owner",
  "tenantName": "Demo Nhà Trọ",
  "tenantSlug": "demo"
}

### Login
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "owner@demo.vn",
  "password": "Demo@123"
}

### Health Check
GET http://localhost:5000/health
```

---

## 9. Architecture Overview

```
rentalos-web/          ← Next.js 15 frontend (TypeScript)
src/
  RentalOS.Domain/     ← Entities, Enums, Domain events
  RentalOS.Application/← CQRS handlers (MediatR), Interfaces, Validators
  RentalOS.Infrastructure/ ← EF Core, External services (R2, Anthropic, Zalo, MoMo, VNPay)
  RentalOS.API/        ← ASP.NET Core 10 REST API
  RentalOS.Worker/     ← Hangfire background job worker
```

---

## 10. Environment Variables Summary

| Variable | Where | Description |
|----------|-------|-------------|
| `ConnectionStrings:DefaultConnection` | appsettings | PostgreSQL connection string |
| `Jwt:Secret` | appsettings | JWT signing key (min 32 chars) |
| `CloudflareR2:AccessKey` | appsettings | R2 file storage access key |
| `CloudflareR2:SecretKey` | appsettings | R2 file storage secret key |
| `Anthropic:ApiKey` | appsettings | Claude AI API key |
| `NEXT_PUBLIC_API_URL` | .env.local | Backend API base URL for frontend |

---

## 11. Deployment (Railway)

The project includes `railway.toml` and `Dockerfile` for Railway deployment.

```bash
railway up
```

Set the same environment variables in the Railway dashboard under **Variables**.
