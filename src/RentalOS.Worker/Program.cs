using RentalOS.Worker;
using RentalOS.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;

var builder = Host.CreateApplicationBuilder(args);

// Add Infrastructure (DB Context, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Add Hangfire Server
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

var host = builder.Build();

// Register Recurring Jobs
using (var scope = host.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    HangfireSetup.RegisterAllJobs(recurringJobManager);
}

host.Run();
