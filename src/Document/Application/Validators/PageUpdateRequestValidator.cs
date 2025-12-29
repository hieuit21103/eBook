using Application.DTOs.Page;
using FluentValidation;

namespace Application.Validators;

public class PageUpdateRequestValidator : AbstractValidator<PageUpdateRequest>
{
    public PageUpdateRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");
    }
}
