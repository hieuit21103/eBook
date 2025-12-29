using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email is not valid.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Username)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .When(x => !string.IsNullOrEmpty(x.Password));
            
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role.")
            .When(x => x.Role.HasValue);
    }
}
