using System.ComponentModel.DataAnnotations;

namespace SportWeb.Models
{
    public class CreateWorkoutModel
    {
        [Required(ErrorMessage = "Name is required")]
        string Name { get; set; }
    }
}
