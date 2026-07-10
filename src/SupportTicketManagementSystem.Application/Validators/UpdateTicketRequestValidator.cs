using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Validators;

public class UpdateTicketRequestValidator : AbstractValidator<UpdateTicketRequest>
{
    public UpdateTicketRequestValidator(
        ITicketRepository ticketRepository,
        IValidator<UpdateTicketDto> updateTicketDtoValidator)
    {
        RuleFor(x => x.TicketId).ValidTicketId(ticketRepository);

        RuleFor(x => x.Dto)
            .NotNull().WithMessage(ValidationMessages.Required("Request body"))
            .SetValidator(updateTicketDtoValidator);

        RuleFor(x => x)
            .CustomAsync(async (request, context, cancellation) =>
            {
                var currentStatus = await ticketRepository.GetStatusAsync(request.TicketId, cancellation);

                if (!currentStatus.HasValue)
                {
                    return;
                }

                if (TicketStatusWorkflow.CanTransition(currentStatus.Value, request.Dto.Status))
                {
                    return;
                }

                var allowedTransitions = TicketStatusWorkflow.GetAllowedTransitions(currentStatus.Value);
                var allowedMessage = allowedTransitions.Count == 0
                    ? "none (terminal status)"
                    : string.Join(", ", allowedTransitions);

                context.AddFailure(
                    nameof(UpdateTicketDto.Status),
                    $"Cannot transition from '{currentStatus.Value}' to '{request.Dto.Status}'. Allowed transitions: {allowedMessage}.");
            });
    }
}
