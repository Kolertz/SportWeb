using System.ComponentModel.DataAnnotations;
using SportWeb.Attributes;
namespace SportWeb.Models
{
    public class EditProfileViewModel
    {
        
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string? Name { get; set; }

        //[Required(ErrorMessage = "Password is required")]
        [Length(6, 100, ErrorMessage = "The {0} must be at least {1} and at max {2} characters long.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        //[Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
        public string? Avatar { get; set; }
        public IFormFile? FileUpload { get; set; }
        public string? Description { get; set; }
        public bool IsPublicFavourites { get; set; } = false;
    }
}
