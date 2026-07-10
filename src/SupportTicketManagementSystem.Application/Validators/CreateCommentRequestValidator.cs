using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;

namespace SupportTicketManagementSystem.Application.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator(
        ITicketRepository ticketRepository,
        IValidator<CreateCommentDto> createCommentDtoValidator)
    {
        RuleFor(x => x.TicketId).ValidTicketId(ticketRepository);

        RuleFor(x => x.Dto)
            .NotNull().WithMessage(ValidationMessages.Required("Request body"))
            .SetValidator(createCommentDtoValidator);
    }
}
