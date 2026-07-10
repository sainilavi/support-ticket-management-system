using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;

namespace SupportTicketManagementSystem.Application.Validators;

public class GetCommentsByTicketRequestValidator : AbstractValidator<GetCommentsByTicketRequest>
{
    public GetCommentsByTicketRequestValidator(ITicketRepository ticketRepository)
    {
        RuleFor(x => x.TicketId).ValidTicketId(ticketRepository);
    }
}
