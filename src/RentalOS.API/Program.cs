using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application;
using RentalOS.Infrastructure;
using RentalOS.API.Middleware;
using RentalOS.API.Hubs;
using Serilog;
using Hangfire;
using Hangfire.PostgreSql;
using FluentValidation.AspNetCore;
using FluentValidation;
using System.Reflection;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;

// Configure QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Logging
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

// 2. Layered DI (Application & Infrastructure)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Web API & Controllers
builder.Services.AddControllers();

// 4. MediatR & FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(RentalOS.Application.DependencyInjection).Assembly);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RentalOS.Application.DependencyInjection).Assembly));

// 5. SignalR
builder.Services.AddSignalR();

// 6. Caching
builder.Services.AddMemoryCache();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
});

// 7. Hangfire (Background Jobs)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

// 8. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 50;
    });

    options.AddFixedWindowLimiter("ai", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
});

// 9. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["https://*.rental.vn"])
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 10. Health Checks
var healthChecks = builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
var healthRedisConn = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(healthRedisConn))
    healthChecks.AddRedis(healthRedisConn);

// 11. Authentication & JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = System.Text.Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "TemporarySecretForSeeding_MustBeUpdatedInProd");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero
    };
    // SignalR sends the token as ?access_token= query param (can't set headers on WebSocket/SSE)
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) &&
                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// 12. Swagger / OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseHealthChecks("/health/liveness"); // Basic live check

// app.UseHttpsRedirection(); // Disabled for local HTTP dev
app.UseRouting();
app.UseCors("DefaultCors"); // Must be between UseRouting and UseAuthentication
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "RentalOS API";
        options.AddHttpAuthentication("Bearer", _ => { });
        options.AddPreferredSecuritySchemes("Bearer");
    });
}

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications").RequireCors("DefaultCors");

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RentalOS.Infrastructure.Persistence.ApplicationDbContext>();
    try { await db.Database.MigrateAsync(); }
    catch (Exception ex) { Log.Warning(ex, "Auto-migration failed (may already be applied)."); }
}

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try { await RentalOS.Infrastructure.Persistence.DatabaseSeeder.SeedAsync(services); }
    catch (Exception ex) { Log.Error(ex, "Seeding failed."); }
}

// Hangfire Dashboard (Protected)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

app.Run();


