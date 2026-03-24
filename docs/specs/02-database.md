# RentalOS — 02. Database Schema (16 bảng)

---

## Multi-tenant Strategy

- **public schema**: bảng dùng chung toàn hệ thống (`tenants`, `subscription_payments`)
- **tenant_{slug} schema**: mỗi tenant có schema riêng, hoàn toàn tách biệt
- Middleware tự động `SET search_path TO tenant_{slug}, public` trên mỗi request

---

## PUBLIC SCHEMA (2 bảng)

### Bảng 1: `public.tenants`

```sql
CREATE TABLE public.tenants (
  id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  slug              VARCHAR(50)  UNIQUE NOT NULL,
  name              VARCHAR(200) NOT NULL,
  owner_email       VARCHAR(255) UNIQUE NOT NULL,
  owner_name        VARCHAR(200) NOT NULL,
  phone             VARCHAR(20),
  plan              VARCHAR(20)  NOT NULL DEFAULT 'trial',
  -- trial | starter | pro | business
  plan_expires_at   TIMESTAMPTZ,
  trial_ends_at     TIMESTAMPTZ,
  schema_name       VARCHAR(60)  NOT NULL,
  is_active         BOOLEAN      NOT NULL DEFAULT true,
  onboarding_done   BOOLEAN      NOT NULL DEFAULT false,
  created_at        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
  updated_at        TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_tenants_slug ON public.tenants(slug);
CREATE INDEX idx_tenants_owner_email ON public.tenants(owner_email);
```

### Bảng 2: `public.subscription_payments`

```sql
CREATE TABLE public.subscription_payments (
  id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tenant_id         UUID NOT NULL REFERENCES public.tenants(id),
  plan              VARCHAR(20)   NOT NULL,
  amount            DECIMAL(12,2) NOT NULL,
  method            VARCHAR(20)   NOT NULL,  -- momo | vnpay
  provider_ref      VARCHAR(200),
  status            VARCHAR(20)   NOT NULL DEFAULT 'pending',
  -- pending | success | failed
  billing_from      DATE NOT NULL,
  billing_to        DATE NOT NULL,
  paid_at           TIMESTAMPTZ,
  created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_sub_payments_tenant ON public.subscription_payments(tenant_id);
```

---

## PER-TENANT SCHEMA (14 bảng)

> Tất cả bảng sau đây tạo trong schema `tenant_{slug}`

### Bảng 3: `users`

```sql
CREATE TABLE users (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email           VARCHAR(255) UNIQUE NOT NULL,
  password_hash   VARCHAR(255) NOT NULL,
  full_name       VARCHAR(200) NOT NULL,
  phone           VARCHAR(20),
  role            VARCHAR(20)  NOT NULL,  -- owner | manager | staff
  avatar_url      VARCHAR(500),
  is_active       BOOLEAN      NOT NULL DEFAULT true,
  invite_token    VARCHAR(100),           -- NULL setelah accepted
  invite_expires_at TIMESTAMPTZ,
  last_login_at   TIMESTAMPTZ,
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_invite_token ON users(invite_token) WHERE invite_token IS NOT NULL;
```

### Bảng 4: `properties`

```sql
CREATE TABLE properties (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name          VARCHAR(200) NOT NULL,
  address       TEXT NOT NULL,
  province      VARCHAR(100),
  district      VARCHAR(100),
  ward          VARCHAR(100),
  lat           DECIMAL(10,7),  -- tọa độ GPS
  lng           DECIMAL(10,7),
  description   TEXT,
  cover_image   VARCHAR(500),
  images        JSONB NOT NULL DEFAULT '[]',
  total_floors  INT   NOT NULL DEFAULT 1,
  is_active     BOOLEAN NOT NULL DEFAULT true,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

### Bảng 5: `rooms`

```sql
CREATE TABLE rooms (
  id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  property_id         UUID NOT NULL REFERENCES properties(id),
  room_number         VARCHAR(20) NOT NULL,
  floor               INT  NOT NULL DEFAULT 1,
  area_sqm            DECIMAL(6,2),
  base_price          DECIMAL(12,2) NOT NULL,
  electricity_price   DECIMAL(8,2)  NOT NULL DEFAULT 3500,
  water_price         DECIMAL(8,2)  NOT NULL DEFAULT 15000,
  service_fee         DECIMAL(10,2) NOT NULL DEFAULT 0,
  internet_fee        DECIMAL(10,2) NOT NULL DEFAULT 0,  -- MỚI
  garbage_fee         DECIMAL(10,2) NOT NULL DEFAULT 0,  -- MỚI
  status              VARCHAR(20) NOT NULL DEFAULT 'available',
  -- available | rented | maintenance | reserved
  amenities           JSONB NOT NULL DEFAULT '[]',
  images              JSONB NOT NULL DEFAULT '[]',
  notes               TEXT,
  maintenance_note    TEXT,                              -- MỚI: lý do bảo trì
  maintenance_since   DATE,                              -- MỚI: ngày bắt đầu bảo trì
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(property_id, room_number)
);

CREATE INDEX idx_rooms_property ON rooms(property_id);
CREATE INDEX idx_rooms_status ON rooms(status);
```

### Bảng 6: `customers`

```sql
CREATE TABLE customers (
  id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  full_name               VARCHAR(200) NOT NULL,
  phone                   VARCHAR(20)  NOT NULL,
  email                   VARCHAR(255),
  id_card_number          VARCHAR(20),
  id_card_image_front     VARCHAR(500),
  id_card_image_back      VARCHAR(500),
  portrait_image          VARCHAR(500),
  date_of_birth           DATE,
  gender                  VARCHAR(10),
  hometown                TEXT,
  current_address         TEXT,                          -- MỚI
  occupation              VARCHAR(200),                  -- MỚI: nghề nghiệp
  workplace               VARCHAR(200),                  -- MỚI: nơi làm việc
  emergency_contact_name  VARCHAR(200),
  emergency_contact_phone VARCHAR(20),
  emergency_contact_relationship VARCHAR(50),            -- MỚI: quan hệ
  is_blacklisted          BOOLEAN NOT NULL DEFAULT false,
  blacklist_reason        TEXT,
  blacklisted_at          TIMESTAMPTZ,
  blacklisted_by          UUID REFERENCES users(id),
  notes                   TEXT,
  created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_customers_phone ON customers(phone);
CREATE INDEX idx_customers_id_card ON customers(id_card_number) WHERE id_card_number IS NOT NULL;
```

### Bảng 7: `contracts`

```sql
CREATE TABLE contracts (
  id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  contract_code       VARCHAR(50) UNIQUE NOT NULL,
  room_id             UUID NOT NULL REFERENCES rooms(id),
  customer_id         UUID NOT NULL REFERENCES customers(id),
  start_date          DATE NOT NULL,
  end_date            DATE NOT NULL,
  monthly_rent        DECIMAL(12,2) NOT NULL,
  deposit_months      INT  NOT NULL DEFAULT 1,
  deposit_amount      DECIMAL(12,2) NOT NULL,
  deposit_paid        BOOLEAN NOT NULL DEFAULT false,    -- MỚI: đã nộp cọc chưa
  deposit_paid_at     TIMESTAMPTZ,                       -- MỚI
  deposit_refunded    DECIMAL(12,2),                     -- MỚI: số tiền đã hoàn cọc
  electricity_price   DECIMAL(8,2),
  water_price         DECIMAL(8,2),
  service_fee         DECIMAL(10,2),
  internet_fee        DECIMAL(10,2) DEFAULT 0,           -- MỚI
  garbage_fee         DECIMAL(10,2) DEFAULT 0,           -- MỚI
  billing_date        INT  NOT NULL DEFAULT 5,
  payment_due_days    INT  NOT NULL DEFAULT 10,
  max_occupants       INT  NOT NULL DEFAULT 2,
  terms               TEXT,
  template_id         VARCHAR(50),                       -- MỚI: template hợp đồng
  pdf_url             VARCHAR(500),
  status              VARCHAR(20) NOT NULL DEFAULT 'active',
  -- active | expired | terminated
  terminated_at       TIMESTAMPTZ,
  termination_reason  TEXT,
  termination_type    VARCHAR(30),                       -- MỚI: normal | breach | mutual
  signed_by_customer  BOOLEAN NOT NULL DEFAULT false,
  signed_at           TIMESTAMPTZ,
  created_by          UUID REFERENCES users(id),
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_contracts_room ON contracts(room_id);
CREATE INDEX idx_contracts_customer ON contracts(customer_id);
CREATE INDEX idx_contracts_status ON contracts(status);
CREATE INDEX idx_contracts_end_date ON contracts(end_date) WHERE status = 'active';
```

### Bảng 8: `contract_co_tenants` ← MỚI

```sql
CREATE TABLE contract_co_tenants (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  contract_id  UUID NOT NULL REFERENCES contracts(id) ON DELETE CASCADE,
  customer_id  UUID NOT NULL REFERENCES customers(id),
  is_primary   BOOLEAN NOT NULL DEFAULT false,  -- true = người đứng tên HĐ
  moved_in_at  DATE,
  moved_out_at DATE,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(contract_id, customer_id)
);

CREATE INDEX idx_co_tenants_contract ON contract_co_tenants(contract_id);
CREATE INDEX idx_co_tenants_customer ON contract_co_tenants(customer_id);
```

### Bảng 9: `invoices`

```sql
CREATE TABLE invoices (
  id                       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  invoice_code             VARCHAR(50) UNIQUE NOT NULL,
  contract_id              UUID NOT NULL REFERENCES contracts(id),
  billing_month            DATE NOT NULL,
  due_date                 DATE NOT NULL,
  -- Điện
  electricity_old          INT  NOT NULL DEFAULT 0,
  electricity_new          INT  NOT NULL DEFAULT 0,
  electricity_price        DECIMAL(8,2) NOT NULL,
  -- Nước
  water_old                INT  NOT NULL DEFAULT 0,
  water_new                INT  NOT NULL DEFAULT 0,
  water_price              DECIMAL(8,2) NOT NULL,
  -- Internet
  internet_fee             DECIMAL(10,2) NOT NULL DEFAULT 0,  -- MỚI
  -- Rác
  garbage_fee              DECIMAL(10,2) NOT NULL DEFAULT 0,  -- MỚI
  -- Tiền phòng & phí
  room_rent                DECIMAL(12,2) NOT NULL,
  service_fee              DECIMAL(10,2) NOT NULL DEFAULT 0,
  other_fees               DECIMAL(10,2) NOT NULL DEFAULT 0,
  other_fees_note          VARCHAR(200),
  discount                 DECIMAL(10,2) NOT NULL DEFAULT 0,
  discount_note            VARCHAR(200),
  -- Tổng (tính bằng trigger hoặc application)
  electricity_amount       DECIMAL(12,2) NOT NULL DEFAULT 0,
  water_amount             DECIMAL(12,2) NOT NULL DEFAULT 0,
  total_amount             DECIMAL(12,2) NOT NULL,
  -- Trạng thái
  status                   VARCHAR(20) NOT NULL DEFAULT 'pending',
  -- pending | paid | overdue | cancelled | partial
  partial_paid_amount      DECIMAL(12,2),                     -- MỚI: đã trả 1 phần
  -- Payment link
  payment_link_token       VARCHAR(100) UNIQUE,
  payment_link_expires_at  TIMESTAMPTZ,
  -- Metadata
  notes                    TEXT,
  pdf_url                  VARCHAR(500),
  meter_image_electricity  VARCHAR(500),                      -- MỚI: ảnh đồng hồ điện
  meter_image_water        VARCHAR(500),                      -- MỚI: ảnh đồng hồ nước
  is_auto_generated        BOOLEAN NOT NULL DEFAULT false,    -- MỚI: do job tạo?
  sent_at                  TIMESTAMPTZ,                       -- MỚI: lúc gửi thông báo
  paid_at                  TIMESTAMPTZ,                       -- MỚI: lúc thanh toán
  created_by               UUID REFERENCES users(id),
  created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at               TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_invoices_contract ON invoices(contract_id);
CREATE INDEX idx_invoices_status ON invoices(status);
CREATE INDEX idx_invoices_billing_month ON invoices(billing_month);
CREATE INDEX idx_invoices_payment_token ON invoices(payment_link_token) WHERE payment_link_token IS NOT NULL;
CREATE UNIQUE INDEX idx_invoices_contract_month ON invoices(contract_id, billing_month)
  WHERE status != 'cancelled';
```

### Bảng 10: `transactions`

```sql
CREATE TABLE transactions (
  id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  invoice_id        UUID REFERENCES invoices(id),
  transaction_code  VARCHAR(100),
  amount            DECIMAL(12,2) NOT NULL,
  method            VARCHAR(20)   NOT NULL,
  -- momo | vnpay | cash | bank_transfer | deposit_refund
  direction         VARCHAR(10)   NOT NULL,  -- income | expense
  category          VARCHAR(30)   NOT NULL DEFAULT 'rent',
  -- rent | deposit | deposit_refund | subscription | other
  provider_ref      VARCHAR(200),
  provider_response JSONB,
  status            VARCHAR(20)   NOT NULL DEFAULT 'success',
  -- success | failed | refunded | pending
  note              VARCHAR(500),
  receipt_url       VARCHAR(500),                            -- MỚI: ảnh biên lai
  recorded_by       UUID REFERENCES users(id),
  paid_at           TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
  created_at        TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_transactions_invoice ON transactions(invoice_id);
CREATE INDEX idx_transactions_provider_ref ON transactions(provider_ref) WHERE provider_ref IS NOT NULL;
CREATE INDEX idx_transactions_paid_at ON transactions(paid_at);
CREATE INDEX idx_transactions_method ON transactions(method);
```

### Bảng 11: `meter_readings`

```sql
CREATE TABLE meter_readings (
  id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  room_id               UUID NOT NULL REFERENCES rooms(id),
  reading_date          DATE NOT NULL,
  electricity_reading   INT  NOT NULL,
  water_reading         INT  NOT NULL,
  electricity_image     VARCHAR(500),
  water_image           VARCHAR(500),
  note                  VARCHAR(200),                        -- MỚI
  recorded_by           UUID REFERENCES users(id),
  created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_meter_readings_room ON meter_readings(room_id);
CREATE INDEX idx_meter_readings_date ON meter_readings(room_id, reading_date DESC);
```

### Bảng 12: `notification_logs` ← MỚI

```sql
CREATE TABLE notification_logs (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  event_type      VARCHAR(50)  NOT NULL,
  -- invoice_created | invoice_overdue | payment_confirmed | contract_expiry | etc.
  channel         VARCHAR(20)  NOT NULL,  -- zalo | sms | email | in_app
  recipient_phone VARCHAR(20),
  recipient_email VARCHAR(255),
  recipient_name  VARCHAR(200),
  subject         VARCHAR(300),
  content         TEXT,
  status          VARCHAR(20)  NOT NULL DEFAULT 'pending',
  -- pending | sent | failed | skipped
  provider_ref    VARCHAR(200),
  error_message   TEXT,
  retry_count     INT          NOT NULL DEFAULT 0,
  sent_at         TIMESTAMPTZ,
  entity_type     VARCHAR(50),  -- invoice | contract | etc.
  entity_id       UUID,
  created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_notif_logs_status ON notification_logs(status, created_at);
CREATE INDEX idx_notif_logs_entity ON notification_logs(entity_type, entity_id);
```

### Bảng 13: `ai_conversations`

```sql
CREATE TABLE ai_conversations (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id     UUID NOT NULL REFERENCES users(id),
  title       VARCHAR(200),                                  -- MỚI: tóm tắt cuộc hội thoại
  messages    JSONB NOT NULL DEFAULT '[]',
  message_count INT  NOT NULL DEFAULT 0,
  last_message_at TIMESTAMPTZ,                              -- MỚI
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_ai_conversations_user ON ai_conversations(user_id);
```

### Bảng 14: `settings`

```sql
CREATE TABLE settings (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  key         VARCHAR(100) UNIQUE NOT NULL,
  value       JSONB NOT NULL,
  updated_by  UUID REFERENCES users(id),
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Keys đầy đủ:
INSERT INTO settings (key, value) VALUES
('payment.momo',            '{"partner_code":"","access_key":"","secret_key":"","is_active":false}'),
('payment.vnpay',           '{"tmn_code":"","hash_secret":"","is_active":false}'),
('payment.bank',            '{"bank_name":"","account_number":"","account_name":"","is_active":false}'),
('notification.zalo',       '{"oa_id":"","access_token":"","refresh_token":"","is_active":false}'),
('notification.sms',        '{"provider":"vnpt","username":"","password":"","brandname":"","is_active":false}'),
('notification.email',      '{"provider":"sendgrid","api_key":"","from":"","from_name":"","is_active":false}'),
('billing.auto',            '{"auto_generate_day":5,"payment_due_days":10,"remind_before_days":[3,1],"remind_overdue_days":[1,3,7]}'),
('billing.prices',          '{"default_electricity":3500,"default_water":15000,"default_service_fee":0}'),
('company.profile',         '{"name":"","address":"","phone":"","email":"","tax_code":"","logo_url":""}'),
('contract.template',       '{"template_id":"default","custom_terms":""}');
```

### Bảng 15: `audit_logs`

```sql
CREATE TABLE audit_logs (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id     UUID REFERENCES users(id),
  user_name   VARCHAR(200),
  action      VARCHAR(100) NOT NULL,
  -- properties.create | rooms.update | invoices.send | etc.
  entity_type VARCHAR(50),
  entity_id   UUID,
  entity_code VARCHAR(100),  -- MỚI: mã dễ đọc (HD-2024-001)
  old_value   JSONB,
  new_value   JSONB,
  ip_address  INET,
  user_agent  TEXT,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_audit_user ON audit_logs(user_id, created_at DESC);
CREATE INDEX idx_audit_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX idx_audit_created ON audit_logs(created_at DESC);
```

### Bảng 16: `payment_link_logs` ← MỚI

```sql
CREATE TABLE payment_link_logs (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  invoice_id   UUID NOT NULL REFERENCES invoices(id),
  ip_address   INET,
  user_agent   TEXT,
  action       VARCHAR(30) NOT NULL,
  -- viewed | momo_initiated | vnpay_initiated | bank_transfer_viewed
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_pay_link_logs_invoice ON payment_link_logs(invoice_id);
```

---

## Tóm tắt

| # | Bảng | Schema | Ghi chú |
|---|------|--------|---------|
| 1 | `tenants` | public | Thông tin tenant |
| 2 | `subscription_payments` | public | Lịch sử mua gói |
| 3 | `users` | per-tenant | Người dùng |
| 4 | `properties` | per-tenant | Nhà trọ |
| 5 | `rooms` | per-tenant | Phòng trọ |
| 6 | `customers` | per-tenant | Khách thuê |
| 7 | `contracts` | per-tenant | Hợp đồng |
| 8 | `contract_co_tenants` | per-tenant | **MỚI** — Người ở chung |
| 9 | `invoices` | per-tenant | Hóa đơn |
| 10 | `transactions` | per-tenant | Giao dịch |
| 11 | `meter_readings` | per-tenant | Chỉ số điện nước |
| 12 | `notification_logs` | per-tenant | **MỚI** — Log thông báo |
| 13 | `ai_conversations` | per-tenant | Lịch sử AI chat |
| 14 | `settings` | per-tenant | Cài đặt |
| 15 | `audit_logs` | per-tenant | Nhật ký hành động |
| 16 | `payment_link_logs` | per-tenant | **MỚI** — Log thanh toán |
