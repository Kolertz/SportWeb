using SportWeb.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SportWeb.Models.Entities
{
    public class Workout
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Workout's title is required")]
        public required string Name { get; set; }

        [EnsureOneExercise]
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
        public ICollection<Superset> Supersets { get; set; } = [];
        public int? AuthorId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        public bool IsPublic { get; set; }
        public string Description { get; set; } = "No Description";
    }
}
