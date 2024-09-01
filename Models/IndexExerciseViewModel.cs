namespace SportWeb.Models
{
    public class IndexExerciseViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? AuthorId { get; set; }
        public string? Username { get; set; }
        public bool IsFavourite { get; set; }
    }
}
