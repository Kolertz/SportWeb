using SportWeb.Models.Entities;

namespace SportWeb.Models
{
    public class ProfileViewModel
    {
        public bool IsUserProfile { get; set; }
        public string? Avatar { get; set; }
        public string Name { get; set; } = "Anonymous";
        public string Description { get; set; } = "No description.";
        public string? Id { get; set; }
        public IEnumerable<Exercise>? AddedExercises { get; set; }
        public int AddedExercisesCount { get; set; }
        public IEnumerable<Workout>? AddedWorkouts { get; set; }
        public int AddedWorkoutsCount { get; set; }
        public int FavouriteExercisesCount { get; set; }
        public IEnumerable<Exercise>? FavouriteExercises { get; set; }
        public bool IsPublicFavourites { get; set; }
    }
}