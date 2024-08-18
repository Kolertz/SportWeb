using SportWeb.Models.Entities;
namespace SportWeb.Models
{
    public class IndexExercisesViewModel
    {
        public PaginationModel? PaginationModel { get; set; }
        public List<Exercise>? Exercises { get; set; }
        public required ExerciseFilterModel FilterModel { get; set; }
    }
}
