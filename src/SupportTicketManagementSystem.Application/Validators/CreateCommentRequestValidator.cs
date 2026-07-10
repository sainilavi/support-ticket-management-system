using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;
using SupportTicketManagementSystem.Domain.Enums;

namespace SupportTicketManagementSystem.Application.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator(
        ITicketRepository ticketRepository,
        IValidator<CreateCommentDto> createCommentDtoValidator)
    {
        RuleFor(x => x.TicketId).ValidTicketId(ticketRepository);

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(ValidationMessages.Required("Request body"))
            .SetValidator(createCommentDtoValidator);

        RuleFor(x => x)
            .MustAsync(async (request, cancellation) =>
            {
                var status = await ticketRepository.GetStatusAsync(request.TicketId, cancellation);
                return status is not (TicketStatus.Closed or TicketStatus.Cancelled);
            })
            .WithMessage("Comments cannot be added to closed or cancelled tickets.")
            .OverridePropertyName(nameof(CreateCommentRequest.TicketId));
    }
}
