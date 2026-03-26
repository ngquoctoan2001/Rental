using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that stamps the PostgreSQL search_path on every connection open
/// based on the resolved schemaName in ITenantContext.
/// </summary>
public sealed class TenantConnectionInterceptor(ITenantContext tenantContext) : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext = tenantContext;

    public override void ConnectionOpened(
        DbConnection connection,
        ConnectionEndEventData eventData)
    {
        SetSchema(connection);
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await SetSchemaAsync(connection, cancellationToken);
    }

    private void SetSchema(DbConnection connection)
    {
        if (!_tenantContext.IsInitialized || string.IsNullOrEmpty(_tenantContext.SchemaName)) return;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SET search_path TO \"{_tenantContext.SchemaName}\", public;";
        cmd.ExecuteNonQuery();
    }

    private async Task SetSchemaAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsInitialized || string.IsNullOrEmpty(_tenantContext.SchemaName)) return;

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SET search_path TO \"{_tenantContext.SchemaName}\", public;";
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
