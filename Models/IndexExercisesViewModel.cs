namespace SportWeb.Models
{
    public class IndexExercisesViewModel
    {
        public required PaginationModel PaginationModel { get; set; }
        public List<IndexExerciseViewModel>? Exercises { get; set; }
        public required ExerciseFilterModel FilterModel { get; set; }
    }
}