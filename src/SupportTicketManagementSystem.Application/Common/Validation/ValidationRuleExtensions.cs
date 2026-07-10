using FluentValidation;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Common.Validation;

public static class ValidationRuleExtensions
{
    public static IRuleBuilderOptions<T, string> ValidTitle<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.Required("Title"))
            .MaximumLength(ValidationConstants.TitleMaxLength)
            .WithMessage(ValidationMessages.MaxLength("Title", ValidationConstants.TitleMaxLength));

    public static IRuleBuilderOptions<T, string> ValidDescription<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(ValidationMessages.Required("Description"))
            .MaximumLength(ValidationConstants.DescriptionMaxLength)
            .WithMessage(ValidationMessages.MaxLength("Description", ValidationConstants.DescriptionMaxLength));

    public static IRuleBuilderOptions<T, TicketPriority> ValidPriority<T>(this IRuleBuilder<T, TicketPriority> ruleBuilder) =>
        ruleBuilder
            .IsInEnum().WithMessage(ValidationMessages.InvalidEnum("Priority"));

    public static IRuleBuilderOptions<T, TicketStatus> ValidStatus<T>(this IRuleBuilder<T, TicketStatus> ruleBuilder) =>
        ruleBuilder
            .IsInEnum().WithMessage(ValidationMessages.InvalidEnum("Status"));

    public static IRuleBuilderOptions<T, int?> ValidAssignedUser<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        IUserRepository userRepository) =>
        ruleBuilder
            .Must(assignedToUserId => !assignedToUserId.HasValue || assignedToUserId.Value > 0)
            .WithMessage(ValidationMessages.GreaterThanZero("AssignedToUserId"))
            .MustAsync(async (assignedToUserId, cancellation) =>
            {
                if (!assignedToUserId.HasValue || assignedToUserId.Value <= 0)
                {
                    return true;
                }

                return await userRepository.ExistsActiveWithAnyRoleAsync(
                    assignedToUserId.Value,
                    [UserRole.Agent, UserRole.Admin],
                    cancellation);
            })
            .WithMessage(ValidationMessages.AssignedUserInvalidRole);

    public static IRuleBuilderOptions<T, int> ValidTicketId<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder
            .GreaterThan(0)
            .WithMessage(ValidationMessages.GreaterThanZero("TicketId"));
}
