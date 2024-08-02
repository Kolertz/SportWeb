namespace SportWeb.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Type { get; set; } = "other";
        public string PictureUrl { get; set; } = $"category.png";
        public List<Exercise>? Exercises { get; set; }
    }
}
