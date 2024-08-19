using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SportWeb.Models.Entities
{
    public class Exercise
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Exercise Title is required")]
        public required string Name { get; set; }
        public string PictureUrl { get; set; } = "picture.png";

        [Required(ErrorMessage = "Description is required")]
        [Length(100, 10000, ErrorMessage = "The {0} must be at least {1} characters long.")]
        public required string Description { get; set; }
        public ExerciseState State { get; set; } = ExerciseState.Pending;
        public List<Category>? Categories { get; set; }
        public int AuthorId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
        public ICollection<User>? UsersWhoFavorited { get; set; }
        public ICollection<WorkoutExercise>? WorkoutExercises { get; set; }
    }
}
