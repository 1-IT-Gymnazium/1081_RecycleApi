namespace Recycle.Api.Models.Parts.Create_Models;

using FluentValidation;
using Recycle.Api.Models.Parts;

public class PartCreateModelValidator : AbstractValidator<PartCreateModel>
{
    public PartCreateModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.")
            .Matches("^[a-zA-Z0-9áéíóúýčďěňřšťžůÁÉÍÓÚÝČĎĚŇŘŠŤŽŮ\\s\\-]+$")
            .WithMessage("Name can only contain letters, digits, spaces and dashes.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.");

        RuleFor(x => x.PicturePath)
            .MaximumLength(500).WithMessage("Picture path is too long.")
            .Must(path =>
                string.IsNullOrWhiteSpace(path) ||
                path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            )
            .WithMessage("Picture must be a .jpg, .jpeg or .png image.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Part type is required.")
            .MaximumLength(50).WithMessage("Part type must be at most 50 characters.");

        RuleFor(x => x.MaterialId)
            .NotEmpty().WithMessage("Material ID is required.");
    }
}
