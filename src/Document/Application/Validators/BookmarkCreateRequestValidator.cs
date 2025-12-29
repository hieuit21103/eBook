using Application.DTOs.Bookmark;
using FluentValidation;

namespace Application.Validators;

public class BookmarkCreateRequestValidator : AbstractValidator<BookmarkCreateRequest>
{
    public BookmarkCreateRequestValidator()
    {
        RuleFor(x => x.PageId)
            .NotEmpty().WithMessage("PageId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
