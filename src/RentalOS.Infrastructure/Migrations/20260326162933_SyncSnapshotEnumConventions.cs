using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalOS.Infrastructure.Migrations
{
    /// <summary>
    /// No-op migration: syncs the EF snapshot with the current model conventions
    /// (enum-to-string conversions for tenant-schema entities, snake_case column names).
    /// Tenant tables are managed by TenantSchemaManager DDL, not EF migrations.
    /// </summary>
    public partial class SyncSnapshotEnumConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty - tenant-schema DDL already uses VARCHAR enum columns
            // and snake_case naming. This migration only updates the EF model snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty
        }
    }
}
