using SportWeb.Models.Entities;
using System.ComponentModel.DataAnnotations;
namespace SportWeb.Models
{
    public class EditExerciseViewModel
    {
        public Exercise Exercise { get; set; }
        public IFormFile? FileUpload { get; set; }
        public required IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Category>? SelectedCategories { get; set; }
    }
}
