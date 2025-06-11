namespace Recycle.Api.Models.Products.Create_Models;

using FluentValidation;
using Recycle.Api.Models.Products;

public class ProductCreateModelValidator : AbstractValidator<ProductCreateModel>
{
    public ProductCreateModelValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.")
            .Matches("^[a-zA-Z0-9áéíóúýčďěňřšťžůÁÉÍÓÚÝČĎĚŇŘŠŤŽŮ\\s\\-]+$")
            .WithMessage("Name can only contain letters, digits, spaces and dashes.");

        RuleFor(x => x.EAN)
            .NotEmpty().WithMessage("EAN is required.")
            .Matches(@"^\d{13}$").WithMessage("EAN must be exactly 13 digits."); // EAN-13 standard

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.");

        RuleFor(x => x.PicturePath)
            .MaximumLength(500).WithMessage("Picture path is too long.");

        RuleFor(x => x.PartIds)
            .NotNull().WithMessage("PartIds are required.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("All PartIds must be valid GUIDs.");
    }
}
