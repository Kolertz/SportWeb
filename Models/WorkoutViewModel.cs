namespace SportWeb.Models
{
    public class WorkoutViewModel
    {
        public int Id { get; set; }

        //[EnsureOneExercise]
        public ICollection<object> WorkoutItems { get; set; } = [];

        public bool IsPublic { get; set; }
        public string Description { get; set; } = "No Description";
        public int? AuthorId { get; set; }
    }
}