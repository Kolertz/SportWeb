using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SportWeb.Attributes
{
    [Obsolete("Use built-in attributes instead")]
    public class PasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string password = (string)value!;
            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return new ValidationResult("Password must contain at least one number.");
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                return new ValidationResult("Password must contain at least one special character.");
            }

            return ValidationResult.Success!;
        }
    }
}
