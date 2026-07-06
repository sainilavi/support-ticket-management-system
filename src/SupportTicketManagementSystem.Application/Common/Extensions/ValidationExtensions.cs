using FluentValidation;
using FluentValidation.Results;
using AppValidationException = SupportTicketManagementSystem.Application.Exceptions.ValidationException;

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
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
}
