using FluentValidation;
using SupportTicketManagementSystem.Application.DTOs.Comments;
using SupportTicketManagementSystem.Application.Interfaces.Repositories;

namespace SupportTicketManagementSystem.Application.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .MustAsync(async (userId, cancellation) =>
                await userRepository.ExistsActiveAsync(userId, cancellation))
            .WithMessage("UserId must reference an active user.");
    }
}
