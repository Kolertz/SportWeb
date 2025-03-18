using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SportWeb.Attributes
{
    [Obsolete("Use built-in attributes instead")]
    public partial class PasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string password = (string)value!;
            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long.");
            }

            if (!UppercaseRegex().IsMatch(password))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            if (!LowercaseRegex().IsMatch(password))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            if (!NumberRegex().IsMatch(password))
            {
                return new ValidationResult("Password must contain at least one number.");
            }

            if (!SpecialCharacterRegex().IsMatch(password))
            {
                return new ValidationResult("Password must contain at least one special character.");
            }

            return ValidationResult.Success!;
        }

        [GeneratedRegex(@"[\W_]")]
        private static partial Regex SpecialCharacterRegex();
        [GeneratedRegex(@"[0-9]")]
        private static partial Regex NumberRegex();
        [GeneratedRegex(@"[a-z]")]
        private static partial Regex LowercaseRegex();
        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex UppercaseRegex();
    }
}