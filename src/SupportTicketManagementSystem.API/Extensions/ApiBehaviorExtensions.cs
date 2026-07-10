using Microsoft.AspNetCore.Mvc;
using SupportTicketManagementSystem.API.Models;
using SupportTicketManagementSystem.Application.Common;
using SupportTicketManagementSystem.Application.Common.Validation;

namespace SupportTicketManagementSystem.API.Extensions;

public static class ApiBehaviorExtensions
{
    private const string InvalidInputMessage = "The input was invalid.";

    public static IMvcBuilder AddCustomApiBehavior(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => NormalizeModelStateKey(entry.Key),
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? InvalidInputMessage
                                : error.ErrorMessage)
                            .Distinct()
                            .ToArray());

                var response = new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ValidationMessages.ValidationFailed,
                    Errors = errors,
                    TraceId = context.HttpContext.TraceIdentifier
                };

                return new BadRequestObjectResult(response)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            };
        });

        return builder;
    }

    private static string NormalizeModelStateKey(string key)
    {
        var normalizedKey = key.StartsWith("$.", StringComparison.Ordinal)
            ? key[2..]
            : key;

        return JsonPropertyNameNormalizer.ToCamelCase(normalizedKey);
    }
}
