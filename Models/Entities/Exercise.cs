namespace SportWeb.Models.Entities
{
    public class Exercise
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string PictureUrl { get; set; } = "picture.png";
        public required string Description { get; set; }
        public ExerciseState State { get; set; } = ExerciseState.Pending;
        public List<Category>? Categories { get; set; }
        public int AuthorId { get; set; }
        public User? User { get; set; }
    }
}
