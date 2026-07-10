using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Comments;

namespace SupportTicketManagementSystem.Application.Validators;

public class GetCommentsByTicketRequestValidator : AbstractValidator<GetCommentsByTicketRequest>
{
    public GetCommentsByTicketRequestValidator()
    {
        RuleFor(x => x.TicketId).ValidTicketId();
    }
}
