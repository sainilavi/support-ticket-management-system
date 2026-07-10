using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Tickets;

namespace SupportTicketManagementSystem.Application.Validators;

public class TicketQueryDtoValidator : AbstractValidator<TicketQueryDto>
{
    public TicketQueryDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .InclusiveBetween(ValidationConstants.MinPageNumber, ValidationConstants.MaxPageNumber)
            .WithMessage(ValidationMessages.MaxValue("PageNumber", ValidationConstants.MaxPageNumber));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(ValidationConstants.MinPageSize, ValidationConstants.MaxPageSize);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage(ValidationMessages.InvalidEnum("Status"))
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Keyword)
            .MaximumLength(ValidationConstants.KeywordMaxLength)
            .WithMessage(ValidationMessages.MaxLength("Keyword", ValidationConstants.KeywordMaxLength))
            .When(x => !string.IsNullOrWhiteSpace(x.Keyword));
    }
}
