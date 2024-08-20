using SportWeb.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportWeb.Attributes
{
    public class EnsureOneExerciseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection<object> exercises && exercises.Any(e => e is Exercise))
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult(ErrorMessage ?? "You must add at least one exercise.");
        }
    }
}
