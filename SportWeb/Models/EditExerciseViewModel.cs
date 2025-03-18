using SportWeb.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace SportWeb.Models
{
    public class EditExerciseViewModel
    {
        [Required]
        public required Exercise Exercise { get; set; }
        public IFormFile? FileUpload { get; set; }
        public IEnumerable<Category> Categories { get; set; } = [];
        public IEnumerable<Category>? SelectedCategories { get; set; }
    }
}