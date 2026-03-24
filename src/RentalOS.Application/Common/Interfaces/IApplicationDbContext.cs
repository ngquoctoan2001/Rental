using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<Property> Properties { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<ContractCoTenant> ContractCoTenants { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<MeterReading> MeterReadings { get; }
    DbSet<NotificationLog> NotificationLogs { get; }
    DbSet<AiConversation> AiConversations { get; }
    DbSet<Setting> Settings { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SubscriptionPayment> SubscriptionPayments { get; }
    DbSet<PaymentLinkLog> PaymentLinkLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }

}
