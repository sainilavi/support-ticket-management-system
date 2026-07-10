namespace SupportTicketManagementSystem.Application.Common.Validation;

public static class ValidationMessages
{
    public const string ValidationFailed = "One or more validation errors occurred.";

    public static string Required(string fieldName) =>
        $"{fieldName} is required.";

    public static string MaxLength(string fieldName, int maxLength) =>
        $"{fieldName} must not exceed {maxLength} characters.";

    public static string GreaterThanZero(string fieldName) =>
        $"{fieldName} must be greater than 0.";

    public static string MaxValue(string fieldName, int maxValue) =>
        $"{fieldName} must not exceed {maxValue}.";

    public static string InvalidEnum(string fieldName) =>
        $"{fieldName} must be a valid value.";

    public const string TicketNotFound = "Ticket does not exist.";
    public const string AssignedUserInvalidRole = "Assigned user must be an active agent or admin.";
    public const string CreatedByUserInvalid = "Created by user must be an active customer.";
    public const string CommentUserInvalid = "User does not exist or is not active.";
}
