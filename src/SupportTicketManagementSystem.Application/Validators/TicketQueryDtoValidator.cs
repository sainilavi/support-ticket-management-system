using FluentValidation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;

namespace SupportTicketManagementSystem.Application.Validators;

public class TicketQueryDtoValidator : AbstractValidator<TicketQueryDto>
{
    public TicketQueryDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Keyword)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
    }
}
