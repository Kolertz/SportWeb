using SportWeb.Models.Entities;

namespace SportWeb.Models
{
    public class ExerciseDetailsViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string PictureUrl { get; set; } = "picture.png";
        public required string Description { get; set; }
        public int? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
        public ExerciseState State { get; set; }
        public bool IsFavourite { get; set; }
    }
}
