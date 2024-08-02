namespace SportWeb.Models
{
    public class ProfileViewModel
    {
        public bool IsUserProfile { get; set; }
        public string? Avatar { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string? Id { get; set; }
    }
}
