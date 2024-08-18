namespace SportWeb.Models
{
    public class WorkoutDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<WorkoutExerciseDto>? WorkoutExercises { get; set; }
        public List<SupersetDto>? Supersets { get; set; }
        public int AuthorId { get; set; }
        public bool IsPublic { get; set; }
        public string? Description { get; set; }
    }

    public class WorkoutExerciseDto
    {
        public int WorkoutId { get; set; }
        public int ExerciseId { get; set; }
        public int Position { get; set; }
    }

    public class SupersetDto
    {
        public int Id { get; set; }
        public int WorkoutId { get; set; }
        public int Position { get; set; }
        public List<WorkoutExerciseDto>? WorkoutExercises { get; set; }
    }
}
