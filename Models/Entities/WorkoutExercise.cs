using System.Text.Json.Serialization;

namespace SportWeb.Models.Entities
{
    public class WorkoutExercise
    {
        public int WorkoutId { get; set; }

        [JsonIgnore]
        public Workout Workout { get; set; } = null!;

        public int ExerciseId { get; set; }

        [JsonIgnore]
        public Exercise Exercise { get; set; } = null!;

        public int Position { get; set; }
        public int? SupersetId { get; set; }

        [JsonIgnore]
        public Superset? Superset { get; set; }
    }
}