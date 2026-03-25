-- In-App Notifications
CREATE TABLE in_app_notifications (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id),
  type VARCHAR(50) NOT NULL,
  title VARCHAR(200) NOT NULL,
  message TEXT NOT NULL,
  is_read BOOLEAN NOT NULL DEFAULT false,
  entity_type VARCHAR(50),
  entity_id UUID,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Notification Logs (Zalo, SMS, Email)
CREATE TABLE notification_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(id),
    channel VARCHAR(20) NOT NULL, -- zalo, sms, email
    event_type VARCHAR(50) NOT NULL, -- invoice_created, etc.
    recipient VARCHAR(200) NOT NULL, -- phone or email
    status VARCHAR(20) NOT NULL, -- success, failed
    error_message TEXT,
    retry_count INT DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_in_app_notifications_user_id ON in_app_notifications(user_id);
CREATE INDEX idx_notification_logs_tenant_id ON notification_logs(tenant_id);
