using System.ComponentModel.DataAnnotations;

namespace SportWeb.Models
{
    public class AddExerciseViewModel
    {
        [Required(ErrorMessage = "Exercise Title is required")]
        public required string Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [Length(100, 10000, ErrorMessage = "The {0} must be at least {1} characters long.")]
        public required string Description { get; set; }
        public IFormFile? FileUpload { get; set; }
    }
}
