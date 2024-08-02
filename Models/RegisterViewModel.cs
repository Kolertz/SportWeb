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
        //[Required(ErrorMessage = "Password is required")]
        //[Length(6, 100, ErrorMessage = "The {0} must be at least {1} characters long.")]
        [DataType(DataType.Password)]
        [Password]
        public required string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }
        /*
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        */
    }
}
