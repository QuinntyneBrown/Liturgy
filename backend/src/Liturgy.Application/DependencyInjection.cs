using System.Reflection;
using FluentValidation;
using Liturgy.Application.Abstractions;
using Liturgy.Application.Behaviors;
using Liturgy.Application.Enforcement;
using Liturgy.Application.Workspaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Liturgy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<EnforcementEngine>();
        services.AddScoped<IWorkspaceAccess, WorkspaceAccess>();

        return services;
    }
}
