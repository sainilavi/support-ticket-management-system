using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Validators;

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator(IValidator<UpdateTicketDto> updateTicketDtoValidator)
    {
        RuleFor(x => x.TicketId)
            .GreaterThan(0)
            .WithMessage(ValidationMessages.GreaterThanZero("TicketId"));

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(ValidationMessages.Required("Request body"))
            .SetValidator(updateTicketDtoValidator);

        RuleFor(x => x)
            .Must(request => TicketStatusWorkflow.CanTransition(request.CurrentStatus, request.Dto!.Status))
            .When(x => x.Dto is not null)
            .WithMessage(request =>
            {
                var allowedTransitions = TicketStatusWorkflow.GetAllowedTransitions(request.CurrentStatus);
                var allowedMessage = allowedTransitions.Count == 0
                    ? "none (terminal status)"
                    : string.Join(", ", allowedTransitions);

                return $"Cannot transition from '{request.CurrentStatus}' to '{request.Dto!.Status}'. Allowed transitions: {allowedMessage}.";
            })
            .When(x => x.Dto is not null)
            .OverridePropertyName(nameof(UpdateTicketDto.Status));
    }
}
