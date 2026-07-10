using FluentValidation;
using SupportTicketManagementSystem.Application.Common.Validation;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;

namespace SupportTicketManagementSystem.Application.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ValidationMessages.Required("Content"))
            .MaximumLength(ValidationConstants.CommentContentMaxLength)
            .WithMessage(ValidationMessages.MaxLength("Content", ValidationConstants.CommentContentMaxLength));

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage(ValidationMessages.GreaterThanZero("UserId"))
            .MustAsync(async (userId, cancellation) =>
                await userRepository.ExistsActiveAsync(userId, cancellation))
            .WithMessage(ValidationMessages.CommentUserInvalid);
    }
}
