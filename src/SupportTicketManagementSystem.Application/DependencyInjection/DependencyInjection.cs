using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SupportTicketManagementSystem.Application.Interfaces.Services;
using SupportTicketManagementSystem.Application.Services;

namespace SupportTicketManagementSystem.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue;

        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();

        return services;
    }
}
