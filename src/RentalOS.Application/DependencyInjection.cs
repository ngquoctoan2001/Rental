using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RentalOS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper, MediatR, FluentValidation, etc.
        // For now, just register the assembly.
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        return services;
    }
}
