using SportWeb.Models.Entities;

namespace SportWeb.Models
{
    public class ProfileViewModel
    {
        public bool IsUserProfile { get; set; }
        public string? Avatar { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Id { get; set; }
        public IEnumerable<Exercise>? AddedExercises { get; set; }
        public int AddedExercisesCount { get; set; }
    }
}
