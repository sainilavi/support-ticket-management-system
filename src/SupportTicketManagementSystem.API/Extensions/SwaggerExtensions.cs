using System.Reflection;
using Microsoft.OpenApi.Models;
using SupportTicketManagementSystem.API.Swagger.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SupportTicketManagementSystem.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("tickets", new OpenApiInfo
            {
                Title = "Tickets API",
                Version = "v1",
                Description = "Endpoints for creating, searching, updating, and deleting support tickets."
            });

            options.SwaggerDoc("comments", new OpenApiInfo
            {
                Title = "Comments API",
                Version = "v1",
                Description = "Endpoints for managing ticket comments."
            });

            options.DocInclusionPredicate((docName, apiDescription) =>
            {
                if (!apiDescription.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) ||
                    string.IsNullOrWhiteSpace(controller))
                {
                    return false;
                }

                return docName switch
                {
                    "tickets" => controller.Equals("Tickets", StringComparison.OrdinalIgnoreCase),
                    "comments" => controller.Equals("Comments", StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            });

            options.TagActionsBy(api =>
            {
                if (api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
                {
                    return [controller];
                }

                return ["Default"];
            });

            options.OrderActionsBy(apiDesc => apiDesc.RelativePath);

            IncludeXmlComments(options);

            options.OperationFilter<SwaggerExampleOperationFilter>();
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/tickets/swagger.json", "Tickets");
            options.SwaggerEndpoint("/swagger/comments/swagger.json", "Comments");
            options.DocumentTitle = "Support Ticket Management System API";
            options.DisplayRequestDuration();
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.EnableDeepLinking();
            options.DefaultModelsExpandDepth(2);
        });

        return app;
    }

    private static void IncludeXmlComments(SwaggerGenOptions options)
    {
        var apiAssembly = Assembly.GetExecutingAssembly();
        var apiXmlPath = Path.Combine(AppContext.BaseDirectory, $"{apiAssembly.GetName().Name}.xml");
        if (File.Exists(apiXmlPath))
        {
            options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
        }

        var applicationXmlPath = Path.Combine(
            AppContext.BaseDirectory,
            "SupportTicketManagementSystem.Application.xml");

        if (File.Exists(applicationXmlPath))
        {
            options.IncludeXmlComments(applicationXmlPath);
        }
    }
}
