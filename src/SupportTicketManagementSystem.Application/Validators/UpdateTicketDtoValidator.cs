using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;

namespace SupportTicketManagementSystem.Application.Validators;

public class UpdateTicketDtoValidator : AbstractValidator<UpdateTicketDto>
{
    public UpdateTicketDtoValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Title).ValidTitle();
        RuleFor(x => x.Description).ValidDescription();
        RuleFor(x => x.Status).ValidStatus();
        RuleFor(x => x.Priority).ValidPriority();
        RuleFor(x => x.AssignedToUserId).ValidAssignedUser(userRepository);
    }
}
