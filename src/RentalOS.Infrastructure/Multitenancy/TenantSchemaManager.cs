using Microsoft.Extensions.Logging;
using Npgsql;
using RentalOS.Application.Common.Interfaces;


namespace RentalOS.Infrastructure.Multitenancy;

/// <summary>
/// Handles PostgreSQL schema creation and DDL for new tenants.
/// Called during tenant registration: creates schema + all 14 per-tenant tables.
/// </summary>
public class TenantSchemaManager(string connectionString, Microsoft.Extensions.Logging.ILogger<TenantSchemaManager> logger) : ITenantSchemaManager
{
    private readonly string _connectionString = connectionString;
    private readonly Microsoft.Extensions.Logging.ILogger<TenantSchemaManager> _logger = logger;



    /// <summary>Creates the per-tenant schema and initialises all 14 base tables.</summary>
    public async Task CreateSchemaAsync(string slug, CancellationToken cancellationToken = default)
    {
        var schemaName = $"tenant_{slug.Replace("-", "_")}";
        var ddl = GetTenantDdlScript(schemaName);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);
        try
        {
            await using var cmd = new NpgsqlCommand(ddl, conn, tx);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            _logger.LogInformation("Created schema {Schema} for tenant {Slug}", schemaName, slug);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create schema for tenant {Slug}", slug);
            throw;
        }
    }

    /// <summary>Returns the full DDL SQL script for 14 per-tenant tables within the given schema.</summary>
    public static string GetTenantDdlScript(string schemaName) => _tenantDdlTemplate.Replace("{SCHEMA}", schemaName);

    private const string _tenantDdlTemplate = """
        CREATE SCHEMA IF NOT EXISTS "{SCHEMA}";
        SET search_path TO "{SCHEMA}";

        -- Bảng 3: users
        CREATE TABLE IF NOT EXISTS users (
          id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          email             VARCHAR(255) UNIQUE NOT NULL,
          password_hash     VARCHAR(255) NOT NULL,
          full_name         VARCHAR(200) NOT NULL,
          phone             VARCHAR(20),
          role              VARCHAR(20) NOT NULL,
          avatar_url        VARCHAR(500),
          is_active         BOOLEAN NOT NULL DEFAULT true,
          invite_token      VARCHAR(100),
          invite_expires_at TIMESTAMPTZ,
          last_login_at     TIMESTAMPTZ,
          created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
        CREATE INDEX IF NOT EXISTS idx_users_invite_token ON users(invite_token) WHERE invite_token IS NOT NULL;

        -- Bảng 4: properties
        CREATE TABLE IF NOT EXISTS properties (
          id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          name          VARCHAR(200) NOT NULL,
          address       TEXT NOT NULL,
          province      VARCHAR(100),
          district      VARCHAR(100),
          ward          VARCHAR(100),
          lat           DECIMAL(10,7),
          lng           DECIMAL(10,7),
          description   TEXT,
          cover_image   VARCHAR(500),
          images        JSONB NOT NULL DEFAULT '[]',
          total_floors  INT NOT NULL DEFAULT 1,
          is_active     BOOLEAN NOT NULL DEFAULT true,
          created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );

        -- Bảng 5: rooms
        CREATE TABLE IF NOT EXISTS rooms (
          id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          property_id         UUID NOT NULL REFERENCES properties(id),
          room_number         VARCHAR(20) NOT NULL,
          floor               INT NOT NULL DEFAULT 1,
          area_sqm            DECIMAL(6,2),
          base_price          DECIMAL(12,2) NOT NULL,
          electricity_price   DECIMAL(8,2) NOT NULL DEFAULT 3500,
          water_price         DECIMAL(8,2) NOT NULL DEFAULT 15000,
          service_fee         DECIMAL(10,2) NOT NULL DEFAULT 0,
          internet_fee        DECIMAL(10,2) NOT NULL DEFAULT 0,
          garbage_fee         DECIMAL(10,2) NOT NULL DEFAULT 0,
          status              VARCHAR(20) NOT NULL DEFAULT 'available',
          amenities           JSONB NOT NULL DEFAULT '[]',
          images              JSONB NOT NULL DEFAULT '[]',
          notes               TEXT,
          maintenance_note    TEXT,
          maintenance_since   DATE,
          created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          UNIQUE(property_id, room_number)
        );
        CREATE INDEX IF NOT EXISTS idx_rooms_property ON rooms(property_id);
        CREATE INDEX IF NOT EXISTS idx_rooms_status ON rooms(status);

        -- Bảng 6: customers
        CREATE TABLE IF NOT EXISTS customers (
          id                              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          full_name                       VARCHAR(200) NOT NULL,
          phone                           VARCHAR(20) NOT NULL,
          email                           VARCHAR(255),
          id_card_number                  VARCHAR(20),
          id_card_image_front             VARCHAR(500),
          id_card_image_back              VARCHAR(500),
          portrait_image                  VARCHAR(500),
          date_of_birth                   DATE,
          gender                          VARCHAR(10),
          hometown                        TEXT,
          current_address                 TEXT,
          occupation                      VARCHAR(200),
          workplace                       VARCHAR(200),
          emergency_contact_name          VARCHAR(200),
          emergency_contact_phone         VARCHAR(20),
          emergency_contact_relationship  VARCHAR(50),
          is_blacklisted                  BOOLEAN NOT NULL DEFAULT false,
          blacklist_reason                TEXT,
          blacklisted_at                  TIMESTAMPTZ,
          blacklisted_by                  UUID REFERENCES users(id),
          notes                           TEXT,
          created_at                      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          updated_at                      TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_customers_phone ON customers(phone);
        CREATE INDEX IF NOT EXISTS idx_customers_id_card ON customers(id_card_number) WHERE id_card_number IS NOT NULL;

        -- Bảng 7: contracts
        CREATE TABLE IF NOT EXISTS contracts (
          id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          contract_code       VARCHAR(50) UNIQUE NOT NULL,
          room_id             UUID NOT NULL REFERENCES rooms(id),
          customer_id         UUID NOT NULL REFERENCES customers(id),
          start_date          DATE NOT NULL,
          end_date            DATE NOT NULL,
          monthly_rent        DECIMAL(12,2) NOT NULL,
          deposit_months      INT NOT NULL DEFAULT 1,
          deposit_amount      DECIMAL(12,2) NOT NULL,
          deposit_paid        BOOLEAN NOT NULL DEFAULT false,
          deposit_paid_at     TIMESTAMPTZ,
          deposit_refunded    DECIMAL(12,2),
          electricity_price   DECIMAL(8,2),
          water_price         DECIMAL(8,2),
          service_fee         DECIMAL(10,2),
          internet_fee        DECIMAL(10,2) DEFAULT 0,
          garbage_fee         DECIMAL(10,2) DEFAULT 0,
          billing_date        INT NOT NULL DEFAULT 5,
          payment_due_days    INT NOT NULL DEFAULT 10,
          max_occupants       INT NOT NULL DEFAULT 2,
          terms               TEXT,
          template_id         VARCHAR(50),
          pdf_url             VARCHAR(500),
          status              VARCHAR(20) NOT NULL DEFAULT 'active',
          terminated_at       TIMESTAMPTZ,
          termination_reason  TEXT,
          termination_type    VARCHAR(30),
          signed_by_customer  BOOLEAN NOT NULL DEFAULT false,
          signed_at           TIMESTAMPTZ,
          created_by          UUID REFERENCES users(id),
          created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_contracts_room ON contracts(room_id);
        CREATE INDEX IF NOT EXISTS idx_contracts_customer ON contracts(customer_id);
        CREATE INDEX IF NOT EXISTS idx_contracts_status ON contracts(status);
        CREATE INDEX IF NOT EXISTS idx_contracts_end_date ON contracts(end_date) WHERE status = 'active';

        -- Bảng 8: contract_co_tenants
        CREATE TABLE IF NOT EXISTS contract_co_tenants (
          id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          contract_id  UUID NOT NULL REFERENCES contracts(id) ON DELETE CASCADE,
          customer_id  UUID NOT NULL REFERENCES customers(id),
          is_primary   BOOLEAN NOT NULL DEFAULT false,
          moved_in_at  DATE,
          moved_out_at DATE,
          created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          UNIQUE(contract_id, customer_id)
        );
        CREATE INDEX IF NOT EXISTS idx_co_tenants_contract ON contract_co_tenants(contract_id);
        CREATE INDEX IF NOT EXISTS idx_co_tenants_customer ON contract_co_tenants(customer_id);

        -- Bảng 9: invoices
        CREATE TABLE IF NOT EXISTS invoices (
          id                       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          invoice_code             VARCHAR(50) UNIQUE NOT NULL,
          contract_id              UUID NOT NULL REFERENCES contracts(id),
          billing_month            DATE NOT NULL,
          due_date                 DATE NOT NULL,
          electricity_old          INT NOT NULL DEFAULT 0,
          electricity_new          INT NOT NULL DEFAULT 0,
          electricity_price        DECIMAL(8,2) NOT NULL,
          water_old                INT NOT NULL DEFAULT 0,
          water_new                INT NOT NULL DEFAULT 0,
          water_price              DECIMAL(8,2) NOT NULL,
          internet_fee             DECIMAL(10,2) NOT NULL DEFAULT 0,
          garbage_fee              DECIMAL(10,2) NOT NULL DEFAULT 0,
          room_rent                DECIMAL(12,2) NOT NULL,
          service_fee              DECIMAL(10,2) NOT NULL DEFAULT 0,
          other_fees               DECIMAL(10,2) NOT NULL DEFAULT 0,
          other_fees_note          VARCHAR(200),
          discount                 DECIMAL(10,2) NOT NULL DEFAULT 0,
          discount_note            VARCHAR(200),
          electricity_amount       DECIMAL(12,2) NOT NULL DEFAULT 0,
          water_amount             DECIMAL(12,2) NOT NULL DEFAULT 0,
          total_amount             DECIMAL(12,2) NOT NULL,
          status                   VARCHAR(20) NOT NULL DEFAULT 'pending',
          partial_paid_amount      DECIMAL(12,2),
          payment_link_token       VARCHAR(100) UNIQUE,
          payment_link_expires_at  TIMESTAMPTZ,
          notes                    TEXT,
          pdf_url                  VARCHAR(500),
          meter_image_electricity  VARCHAR(500),
          meter_image_water        VARCHAR(500),
          is_auto_generated        BOOLEAN NOT NULL DEFAULT false,
          sent_at                  TIMESTAMPTZ,
          paid_at                  TIMESTAMPTZ,
          created_by               UUID REFERENCES users(id),
          created_at               TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          updated_at               TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_invoices_contract ON invoices(contract_id);
        CREATE INDEX IF NOT EXISTS idx_invoices_status ON invoices(status);
        CREATE INDEX IF NOT EXISTS idx_invoices_billing_month ON invoices(billing_month);
        CREATE INDEX IF NOT EXISTS idx_invoices_payment_token ON invoices(payment_link_token) WHERE payment_link_token IS NOT NULL;
        CREATE UNIQUE INDEX IF NOT EXISTS idx_invoices_contract_month ON invoices(contract_id, billing_month) WHERE status != 'cancelled';

        -- Bảng 10: transactions
        CREATE TABLE IF NOT EXISTS transactions (
          id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          invoice_id        UUID REFERENCES invoices(id),
          transaction_code  VARCHAR(100),
          amount            DECIMAL(12,2) NOT NULL,
          method            VARCHAR(20) NOT NULL,
          direction         VARCHAR(10) NOT NULL,
          category          VARCHAR(30) NOT NULL DEFAULT 'rent',
          provider_ref      VARCHAR(200),
          provider_response JSONB,
          status            VARCHAR(20) NOT NULL DEFAULT 'success',
          note              VARCHAR(500),
          receipt_url       VARCHAR(500),
          recorded_by       UUID REFERENCES users(id),
          paid_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_transactions_invoice ON transactions(invoice_id);
        CREATE INDEX IF NOT EXISTS idx_transactions_provider_ref ON transactions(provider_ref) WHERE provider_ref IS NOT NULL;
        CREATE INDEX IF NOT EXISTS idx_transactions_paid_at ON transactions(paid_at);
        CREATE INDEX IF NOT EXISTS idx_transactions_method ON transactions(method);

        -- Bảng 11: meter_readings
        CREATE TABLE IF NOT EXISTS meter_readings (
          id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          room_id               UUID NOT NULL REFERENCES rooms(id),
          reading_date          DATE NOT NULL,
          electricity_reading   INT NOT NULL,
          water_reading         INT NOT NULL,
          electricity_image     VARCHAR(500),
          water_image           VARCHAR(500),
          note                  VARCHAR(200),
          recorded_by           UUID REFERENCES users(id),
          created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_meter_readings_room ON meter_readings(room_id);
        CREATE INDEX IF NOT EXISTS idx_meter_readings_date ON meter_readings(room_id, reading_date DESC);

        -- Bảng 12: notification_logs
        CREATE TABLE IF NOT EXISTS notification_logs (
          id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          event_type      VARCHAR(50) NOT NULL,
          channel         VARCHAR(20) NOT NULL,
          recipient_phone VARCHAR(20),
          recipient_email VARCHAR(255),
          recipient_name  VARCHAR(200),
          subject         VARCHAR(300),
          content         TEXT,
          status          VARCHAR(20) NOT NULL DEFAULT 'pending',
          provider_ref    VARCHAR(200),
          error_message   TEXT,
          retry_count     INT NOT NULL DEFAULT 0,
          sent_at         TIMESTAMPTZ,
          entity_type     VARCHAR(50),
          entity_id       UUID,
          created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_notif_logs_status ON notification_logs(status, created_at);
        CREATE INDEX IF NOT EXISTS idx_notif_logs_entity ON notification_logs(entity_type, entity_id);

        -- Bảng 13: ai_conversations
        CREATE TABLE IF NOT EXISTS ai_conversations (
          id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          user_id          UUID NOT NULL REFERENCES users(id),
          title            VARCHAR(200),
          messages         JSONB NOT NULL DEFAULT '[]',
          message_count    INT NOT NULL DEFAULT 0,
          last_message_at  TIMESTAMPTZ,
          created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
          updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_ai_conversations_user ON ai_conversations(user_id);

        -- Bảng 14: settings
        CREATE TABLE IF NOT EXISTS settings (
          id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          key         VARCHAR(100) UNIQUE NOT NULL,
          value       JSONB NOT NULL,
          updated_by  UUID REFERENCES users(id),
          updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );

        -- Bảng 15: audit_logs
        CREATE TABLE IF NOT EXISTS audit_logs (
          id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          user_id     UUID REFERENCES users(id),
          user_name   VARCHAR(200),
          action      VARCHAR(100) NOT NULL,
          entity_type VARCHAR(50),
          entity_id   UUID,
          entity_code VARCHAR(100),
          old_value   JSONB,
          new_value   JSONB,
          ip_address  INET,
          user_agent  TEXT,
          created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_audit_user ON audit_logs(user_id, created_at DESC);
        CREATE INDEX IF NOT EXISTS idx_audit_entity ON audit_logs(entity_type, entity_id);
        CREATE INDEX IF NOT EXISTS idx_audit_created ON audit_logs(created_at DESC);

        -- Bảng 16: payment_link_logs
        CREATE TABLE IF NOT EXISTS payment_link_logs (
          id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
          invoice_id   UUID NOT NULL REFERENCES invoices(id),
          ip_address   INET,
          user_agent   TEXT,
          action       VARCHAR(30) NOT NULL,
          created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_pay_link_logs_invoice ON payment_link_logs(invoice_id);

        -- Seed default settings
        INSERT INTO settings (key, value) VALUES
          ('payment.momo',      '{"partner_code":"","access_key":"","secret_key":"","is_active":false}'::jsonb),
          ('payment.vnpay',     '{"tmn_code":"","hash_secret":"","is_active":false}'::jsonb),
          ('payment.bank',      '{"bank_name":"","account_number":"","account_name":"","is_active":false}'::jsonb),
          ('notification.zalo', '{"oa_id":"","access_token":"","refresh_token":"","is_active":false}'::jsonb),
          ('notification.sms',  '{"provider":"vnpt","username":"","password":"","brandname":"","is_active":false}'::jsonb),
          ('notification.email','{"provider":"sendgrid","api_key":"","from":"","from_name":"","is_active":false}'::jsonb),
          ('billing.auto',      '{"auto_generate_day":5,"payment_due_days":10,"remind_before_days":[3,1],"remind_overdue_days":[1,3,7]}'::jsonb),
          ('billing.prices',    '{"default_electricity":3500,"default_water":15000,"default_service_fee":0}'::jsonb),
          ('company.profile',   '{"name":"","address":"","phone":"","email":"","tax_code":"","logo_url":""}'::jsonb),
          ('contract.template', '{"template_id":"default","custom_terms":""}'::jsonb)
        ON CONFLICT (key) DO NOTHING;
        """;
}
