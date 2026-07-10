namespace SupportTicketManagementSystem.Application.Exceptions;

public class ValidationException : Exception
{
    public const string DefaultMessage = Common.Validation.ValidationMessages.ValidationFailed;

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
            { ToCamelCase(propertyName), new[] { errorMessage } }
        };
    }

    private static string ToCamelCase(string propertyName) =>
        string.IsNullOrEmpty(propertyName) || char.IsLower(propertyName[0])
            ? propertyName
            : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
}
