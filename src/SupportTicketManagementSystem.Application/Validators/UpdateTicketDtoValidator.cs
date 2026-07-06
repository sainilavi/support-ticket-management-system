using FluentValidation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Validators;

public class UpdateTicketDtoValidator : AbstractValidator<UpdateTicketDto>
{
    public UpdateTicketDtoValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.AssignedToUserId)
            .MustAsync(async (assignedToUserId, cancellation) =>
            {
                if (!assignedToUserId.HasValue)
                {
                    return true;
                }

                return await userRepository.ExistsActiveWithAnyRoleAsync(
                    assignedToUserId.Value,
                    cancellation,
                    UserRole.Agent,
                    UserRole.Admin);
            })
            .WithMessage("AssignedToUserId must reference an active agent or admin when provided.");
    }
}
