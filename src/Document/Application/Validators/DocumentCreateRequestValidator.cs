using Application.DTOs.Document;
using FluentValidation;

namespace Application.Validators;

public class DocumentCreateRequestValidator : AbstractValidator<DocumentCreateRequest>
{
    public DocumentCreateRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.");
    }
}
