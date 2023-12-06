using System.ComponentModel.DataAnnotations;

namespace core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute
{
    protected override ValidationResult IsValid(object? givenString, ValidationContext validationContext)
    {
        if (IsToxic((string?)givenString))
        {
            return new ValidationResult("Message is toxic.");
        }

        return ValidationResult.Success!;
    }

    public bool IsToxic(string? message)
    {
        return false;
    }
}