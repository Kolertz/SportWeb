namespace SportWeb.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Type { get; set; } = "Other";
        public List<Exercise> Exercises { get; set; } = [];
    }
}