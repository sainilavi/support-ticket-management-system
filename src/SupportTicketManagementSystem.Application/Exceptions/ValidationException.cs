using SupportTicketManagementSystem.Application.Common;
using SupportTicketManagementSystem.Application.Common.Validation;

namespace SupportTicketManagementSystem.Application.Exceptions;

public class ValidationException : Exception
{
    public const string DefaultMessage = ValidationMessages.ValidationFailed;

    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base(DefaultMessage)
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(DefaultMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { JsonPropertyNameNormalizer.ToCamelCase(propertyName), new[] { errorMessage } }
        };
    }
}
