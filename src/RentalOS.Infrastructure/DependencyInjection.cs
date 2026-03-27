using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Infrastructure.Multitenancy;
using RentalOS.Infrastructure.Persistence;
using RentalOS.Infrastructure.Persistence.Interceptors;
using RentalOS.Infrastructure.Services;
using RentalOS.Infrastructure.Services.AI;
using RentalOS.Infrastructure.Services.Notifications;
using RentalOS.Infrastructure.Services.Payments;
using RentalOS.Infrastructure.Services.Storage;
using RentalOS.Infrastructure.Services.Pdf;
using RentalOS.Infrastructure.BackgroundJobs;
using Npgsql;
using System.Data;
using StackExchange.Redis;


namespace RentalOS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(connectionString));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<TenantConnectionInterceptor>();
        
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
            var connectionInterceptor = sp.GetRequiredService<TenantConnectionInterceptor>();

            options.UseNpgsql(connectionString)
                   .AddInterceptors(auditInterceptor, connectionInterceptor);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());


        services.AddIdentity<RentalOS.Domain.Entities.User, RentalOS.Domain.Entities.ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Set to true for production
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<ITenantSchemaManager>(sp => 
            new TenantSchemaManager(connectionString!, sp.GetRequiredService<ILogger<TenantSchemaManager>>()));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IR2StorageService, R2StorageService>();
        services.AddScoped<IOcrService, OcrService>();
        
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IContractPdfService, ContractPdfService>();
        services.AddScoped<GenerateContractPdfJob>();

        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        services.AddScoped<SendInvoiceNotificationJob>();

        // Notifications
        services.AddScoped<IZaloService, ZaloService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IEmailService>(sp => new EmailService(
            sp.GetRequiredService<IDbConnection>(),
            sp.GetRequiredService<ITenantContext>(),
            sp.GetRequiredService<IConfiguration>(),
            sp.GetRequiredService<ILogger<EmailService>>()
        ));
        services.AddScoped<INotificationService, NotificationService>();

        services.AddHttpClient();
        services.AddScoped<IMoMoService>(sp => new MoMoService(
            sp.GetRequiredService<IApplicationDbContext>(),
            sp.GetRequiredService<ITenantContext>(),
            sp.GetRequiredService<IHttpClientFactory>(),
            sp.GetService<IConnectionMultiplexer>(),
            sp.GetRequiredService<ILogger<MoMoService>>()
        ));
        services.AddScoped<IVNPayService>(sp => new VNPayService(
            sp.GetRequiredService<IApplicationDbContext>(),
            sp.GetRequiredService<ITenantContext>(),
            sp.GetService<IConnectionMultiplexer>(),
            sp.GetRequiredService<IBackgroundJobService>(),
            sp.GetRequiredService<ILogger<VNPayService>>()
        ));
        services.AddScoped<IAiStreamingService, AnthropicService>();

        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        return services;

    }
}
