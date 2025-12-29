using Application.DTOs.Document;
using FluentValidation;

namespace Application.Validators;

public class DocumentUpdateRequestValidator : AbstractValidator<DocumentUpdateRequest>
{
    public DocumentUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.");
    }
}
