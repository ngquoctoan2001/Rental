using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RentalOS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_code = table.Column<string>(type: "text", nullable: true),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    id_card_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_card_image_front = table.Column<string>(type: "text", nullable: true),
                    id_card_image_back = table.Column<string>(type: "text", nullable: true),
                    portrait_image = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: true),
                    hometown = table.Column<string>(type: "text", nullable: true),
                    current_address = table.Column<string>(type: "text", nullable: true),
                    occupation = table.Column<string>(type: "text", nullable: true),
                    workplace = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "text", nullable: true),
                    emergency_contact_relationship = table.Column<string>(type: "text", nullable: true),
                    is_blacklisted = table.Column<bool>(type: "boolean", nullable: false),
                    blacklist_reason = table.Column<string>(type: "text", nullable: true),
                    blacklisted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    blacklisted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "in_app_notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_in_app_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recipient_phone = table.Column<string>(type: "text", nullable: true),
                    recipient_email = table.Column<string>(type: "text", nullable: true),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_ref = table.Column<string>(type: "text", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notification_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    property_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    province = table.Column<string>(type: "text", nullable: true),
                    district = table.Column<string>(type: "text", nullable: true),
                    ward = table.Column<string>(type: "text", nullable: true),
                    lat = table.Column<decimal>(type: "numeric", nullable: true),
                    lng = table.Column<decimal>(type: "numeric", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    cover_image = table.Column<string>(type: "text", nullable: true),
                    images = table.Column<string>(type: "jsonb", nullable: false),
                    total_floors = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_properties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OwnerEmail = table.Column<string>(type: "text", nullable: false),
                    OwnerName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    PlanExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TrialEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SchemaName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OnboardingDone = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    InviteToken = table.Column<string>(type: "text", nullable: true),
                    InviteExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedPropertyIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    floor = table.Column<int>(type: "integer", nullable: false),
                    area_sqm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    electricity_price = table.Column<decimal>(type: "numeric", nullable: false),
                    water_price = table.Column<decimal>(type: "numeric", nullable: false),
                    service_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    internet_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    garbage_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amenities = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    images = table.Column<string>(type: "jsonb", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    maintenance_note = table.Column<string>(type: "text", nullable: true),
                    maintenance_since = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_rooms", x => x.id);
                    table.ForeignKey(
                        name: "f_k_rooms_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscription_payments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    ProviderRef = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    BillingFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    BillingTo = table.Column<DateOnly>(type: "date", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscription_payments_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "public",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Vietnam"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    property_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_addresses", x => x.id);
                    table.ForeignKey(
                        name: "f_k_addresses_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_addresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ai_conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    messages = table.Column<string>(type: "jsonb", nullable: false),
                    message_count = table.Column<int>(type: "integer", nullable: false),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_ai_conversations", x => x.id);
                    table.ForeignKey(
                        name: "f_k_ai_conversations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_maintenance_tasks", x => x.id);
                    table.ForeignKey(
                        name: "f_k_maintenance_tasks_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_maintenance_tasks_users_assigned_to_id",
                        column: x => x.assigned_to_id,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_reviews", x => x.id);
                    table.ForeignKey(
                        name: "f_k_reviews_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contracts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    monthly_rent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    deposit_months = table.Column<int>(type: "integer", nullable: false),
                    deposit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    deposit_paid = table.Column<bool>(type: "boolean", nullable: false),
                    deposit_paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deposit_refunded = table.Column<decimal>(type: "numeric", nullable: true),
                    electricity_price = table.Column<decimal>(type: "numeric", nullable: true),
                    water_price = table.Column<decimal>(type: "numeric", nullable: true),
                    service_fee = table.Column<decimal>(type: "numeric", nullable: true),
                    internet_fee = table.Column<decimal>(type: "numeric", nullable: true),
                    garbage_fee = table.Column<decimal>(type: "numeric", nullable: true),
                    billing_date = table.Column<int>(type: "integer", nullable: false),
                    payment_due_days = table.Column<int>(type: "integer", nullable: false),
                    max_occupants = table.Column<int>(type: "integer", nullable: false),
                    terms = table.Column<string>(type: "text", nullable: true),
                    template_id = table.Column<string>(type: "text", nullable: true),
                    pdf_url = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    terminated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    termination_reason = table.Column<string>(type: "text", nullable: true),
                    termination_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    signed_by_customer = table.Column<bool>(type: "boolean", nullable: false),
                    signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contracts", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contracts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_contracts_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "meter_readings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_date = table.Column<DateOnly>(type: "date", nullable: false),
                    electricity_reading = table.Column<int>(type: "integer", nullable: false),
                    water_reading = table.Column<int>(type: "integer", nullable: false),
                    electricity_image = table.Column<string>(type: "text", nullable: true),
                    water_image = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    recorded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_meter_readings", x => x.id);
                    table.ForeignKey(
                        name: "f_k_meter_readings_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_co_tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    moved_in_at = table.Column<DateOnly>(type: "date", nullable: true),
                    moved_out_at = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_contract_co_tenants", x => x.id);
                    table.ForeignKey(
                        name: "f_k_contract_co_tenants_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_contract_co_tenants_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    contract_id = table.Column<Guid>(type: "uuid", nullable: false),
                    billing_month = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    electricity_old = table.Column<int>(type: "integer", nullable: false),
                    electricity_new = table.Column<int>(type: "integer", nullable: false),
                    electricity_price = table.Column<decimal>(type: "numeric", nullable: false),
                    water_old = table.Column<int>(type: "integer", nullable: false),
                    water_new = table.Column<int>(type: "integer", nullable: false),
                    water_price = table.Column<decimal>(type: "numeric", nullable: false),
                    internet_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    garbage_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    room_rent = table.Column<decimal>(type: "numeric", nullable: false),
                    service_fee = table.Column<decimal>(type: "numeric", nullable: false),
                    other_fees = table.Column<decimal>(type: "numeric", nullable: false),
                    other_fees_note = table.Column<string>(type: "text", nullable: true),
                    discount = table.Column<decimal>(type: "numeric", nullable: false),
                    discount_note = table.Column<string>(type: "text", nullable: true),
                    electricity_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    water_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    partial_paid_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    payment_link_token = table.Column<string>(type: "text", nullable: true),
                    payment_link_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    pdf_url = table.Column<string>(type: "text", nullable: true),
                    meter_image_electricity = table.Column<string>(type: "text", nullable: true),
                    meter_image_water = table.Column<string>(type: "text", nullable: true),
                    is_auto_generated = table.Column<bool>(type: "boolean", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_invoices", x => x.id);
                    table.ForeignKey(
                        name: "f_k_invoices_contracts_contract_id",
                        column: x => x.contract_id,
                        principalTable: "contracts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_link_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    action = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_payment_link_logs", x => x.id);
                    table.ForeignKey(
                        name: "f_k_payment_link_logs_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_code = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    direction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    provider_response = table.Column<string>(type: "jsonb", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    receipt_url = table.Column<string>(type: "text", nullable: true),
                    recorded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_transactions", x => x.id);
                    table.ForeignKey(
                        name: "f_k_transactions_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_addresses_property_id",
                table: "addresses",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "i_x_addresses_user_id",
                table: "addresses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_ai_conversations_user_id",
                table: "ai_conversations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "i_x_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_contract_co_tenants_contract_id_customer_id",
                table: "contract_co_tenants",
                columns: new[] { "contract_id", "customer_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_contract_co_tenants_customer_id",
                table: "contract_co_tenants",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contracts_contract_code",
                table: "contracts",
                column: "contract_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_contracts_customer_id",
                table: "contracts",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "i_x_contracts_end_date",
                table: "contracts",
                column: "end_date",
                filter: "\"status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "i_x_contracts_room_id",
                table: "contracts",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "i_x_customers_id_card_number",
                table: "customers",
                column: "id_card_number");

            migrationBuilder.CreateIndex(
                name: "i_x_customers_phone",
                table: "customers",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "i_x_in_app_notifications_created_at",
                table: "in_app_notifications",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "i_x_in_app_notifications_user_id_is_read",
                table: "in_app_notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "i_x_invoices_contract_id_billing_month",
                table: "invoices",
                columns: new[] { "contract_id", "billing_month" },
                unique: true,
                filter: "\"status\" != 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "i_x_invoices_invoice_code",
                table: "invoices",
                column: "invoice_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_invoices_payment_link_token",
                table: "invoices",
                column: "payment_link_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_maintenance_tasks_assigned_to_id",
                table: "maintenance_tasks",
                column: "assigned_to_id");

            migrationBuilder.CreateIndex(
                name: "i_x_maintenance_tasks_property_id",
                table: "maintenance_tasks",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "i_x_meter_readings_room_id_reading_date",
                table: "meter_readings",
                columns: new[] { "room_id", "reading_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "i_x_notification_logs_created_at",
                table: "notification_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "i_x_payment_link_logs_invoice_id",
                table: "payment_link_logs",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "i_x_properties_owner_id",
                table: "properties",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "i_x_reviews_property_id",
                table: "reviews",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "i_x_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_rooms_property_id_room_number",
                table: "rooms",
                columns: new[] { "property_id", "room_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_settings_group_key",
                table: "settings",
                columns: new[] { "group", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payments_ProviderRef",
                schema: "public",
                table: "subscription_payments",
                column: "ProviderRef");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_payments_TenantId",
                schema: "public",
                table: "subscription_payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                schema: "public",
                table: "tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_transactions_invoice_id",
                table: "transactions",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "i_x_transactions_paid_at",
                table: "transactions",
                column: "paid_at");

            migrationBuilder.CreateIndex(
                name: "i_x_transactions_provider_ref",
                table: "transactions",
                column: "provider_ref");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_InviteToken",
                table: "users",
                column: "InviteToken");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "ai_conversations");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "contract_co_tenants");

            migrationBuilder.DropTable(
                name: "in_app_notifications");

            migrationBuilder.DropTable(
                name: "maintenance_tasks");

            migrationBuilder.DropTable(
                name: "meter_readings");

            migrationBuilder.DropTable(
                name: "notification_logs");

            migrationBuilder.DropTable(
                name: "payment_link_logs");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "subscription_payments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "properties");
        }
    }
}
