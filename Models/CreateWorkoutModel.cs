using System.ComponentModel.DataAnnotations;

namespace SportWeb.Models
{
    public class CreateWorkoutModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
    }
}
