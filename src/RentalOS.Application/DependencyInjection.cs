using MediatR;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using RentalOS.Application.Common.Services;
using RentalOS.Application.Modules.AI.Services;

namespace RentalOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddSingleton<IPlanService, PlanService>();
        services.AddScoped<AiToolHandler>();

        return services;
    }
}
