using System.Text.Json.Serialization;

namespace SportWeb.Models.Entities
{
    public class Superset
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }

        [JsonIgnore]
        public Workout Workout { get; set; } = null!;

        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
        public int Position { get; set; }
    }
}