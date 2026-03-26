namespace RentalOS.Application.Common.Interfaces;

public interface ITenantSchemaManager
{
    Task CreateSchemaAsync(string slug, CancellationToken cancellationToken = default);
    Task PatchSchemaAsync(string slug, CancellationToken cancellationToken = default);
}
