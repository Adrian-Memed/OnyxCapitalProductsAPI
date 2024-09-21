using FluentValidation;
using ProductsWebAPI.Validation;
public class ColourValidator : AbstractValidator<string>
{
    public ColourValidator()
    {
        RuleFor(colour => colour)
            .NotEmpty().WithMessage("Product colour is required.")
            .Must(BeAValidColor).WithMessage("'{PropertyValue}' is not a valid colour.");
    }
    public bool BeAValidColor(string color)
    {
        return ValidColours.ColourSet.Contains(color);
    }
}
