using SportWeb.Attributes;
using System.ComponentModel.DataAnnotations;

namespace SportWeb.Models
{
    public class RegisterViewModel
    {
        
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string? Name { get; set; }

        [UIHint("Email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [UIHint("Password")]
        [DataType(DataType.Password)]
        //[Password(ErrorMessage = "Password does not meet the requirements.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
