namespace SportWeb.Models.Entities
{
    public class Superset
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public Workout Workout { get; set; } = null!;
        public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
        public int Position { get; set; }
    }
}
