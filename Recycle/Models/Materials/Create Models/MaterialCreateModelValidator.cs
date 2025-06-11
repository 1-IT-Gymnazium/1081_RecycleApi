using FluentValidation;

namespace Recycle.Api.Models.Materials;

public class MaterialCreateModelValidator : AbstractValidator<MaterialCreateModel>
{
    public MaterialCreateModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.")
            .Matches("^[a-zA-Z0-9áéíóúýčďěňřšťžůÁÉÍÓÚÝČĎĚŇŘŠŤŽŮ\\s\\-]+$")
            .WithMessage("Name can only contain letters, numbers, spaces, and dashes.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be at most 500 characters.");

        RuleFor(x => x.TrashCanIds)
            .Must(list => list != null && list.All(id => id != Guid.Empty))
            .WithMessage("All TrashCanIds must be valid GUIDs.");

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Material ID is required.");
    }
}
