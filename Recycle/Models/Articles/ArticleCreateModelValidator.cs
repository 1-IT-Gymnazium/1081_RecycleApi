using FluentValidation;

namespace Recycle.Api.Models.Articles;

public class ArticleCreateModelValidator : AbstractValidator<ArticleCreateModel>
{
    public ArticleCreateModelValidator()
    {
        RuleFor(x => x.Heading)
            .NotEmpty().WithMessage("Heading is required.")
            .MaximumLength(200);
        RuleFor(x => x.Annotation)
            .NotEmpty().WithMessage("Annotation is required.")
            .MaximumLength(500);
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text of article is required.");
        RuleFor(x => x.PicturePath)
            .NotNull().WithMessage("Picture is requred.");

    }
}
