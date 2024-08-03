namespace SportWeb.Models.Entities
{
    public class Workout
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Exercise>? Exercises { get; set; }
        public int AuthorId { get; set; }
        public User? User { get; set; }
    }
}
