namespace RentalOS.Application.Common.Interfaces;

public interface ITenantSchemaManager
{
    Task CreateSchemaAsync(string slug, CancellationToken cancellationToken = default);
}
