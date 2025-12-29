using Application.DTOs.Page;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Validators;

public class PageCreateRequestValidator : AbstractValidator<PageCreateRequest>
{
    public PageCreateRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("DocumentId is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Content file is required.");
    }
}
