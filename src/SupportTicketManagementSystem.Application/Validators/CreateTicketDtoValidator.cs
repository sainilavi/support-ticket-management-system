using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Validators;

public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketDtoValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Priority).ValidPriority();

        RuleFor(x => x.CreatedByUserId)
            .GreaterThan(0).WithMessage(ValidationMessages.GreaterThanZero("CreatedByUserId"))
            .MustAsync(async (userId, cancellation) =>
                await userRepository.ExistsActiveWithRoleAsync(userId, UserRole.Customer, cancellation))
            .WithMessage(ValidationMessages.CreatedByUserInvalid);

        RuleFor(x => x.AssignedToUserId).ValidAssignedUser(userRepository);
    }
}
