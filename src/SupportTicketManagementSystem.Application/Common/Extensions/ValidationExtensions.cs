using FluentValidation;
using FluentValidation.Results;
using AppValidationException = SupportTicketManagementSystem.Application.Exceptions.ValidationException;
using SupportTicketManagementSystem.Application.Common;

namespace SupportTicketManagementSystem.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static async Task ValidateDtoAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (!result.IsValid)
        {
            throw new AppValidationException(ToErrorDictionary(result));
        }
    }

    private static IDictionary<string, string[]> ToErrorDictionary(ValidationResult result) =>
        result.Errors
            .GroupBy(error => NormalizePropertyName(error.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

    private static string NormalizePropertyName(string propertyName)
    {
        var camelCaseName = JsonPropertyNameNormalizer.ToCamelCase(propertyName);

        if (camelCaseName.StartsWith("dto.", StringComparison.Ordinal))
        {
            return camelCaseName[4..];
        }

        return camelCaseName;
    }
}
