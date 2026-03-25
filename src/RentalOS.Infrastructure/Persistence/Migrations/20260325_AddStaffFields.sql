-- Migration: Add assigned_property_ids to users table
ALTER TABLE users ADD COLUMN IF NOT EXISTS assigned_property_ids JSONB DEFAULT '[]';

-- Ensure Index for performance if needed
CREATE INDEX IF NOT EXISTS idx_users_assigned_property_ids ON users USING GIN (assigned_property_ids);
