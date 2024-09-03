namespace SportWeb.Models
{
    public class IndexExercisesViewModel
    {
        public PaginationModel? PaginationModel { get; set; }
        public List<IndexExerciseViewModel>? Exercises { get; set; }
        public ExerciseFilterModel FilterModel { get; set; }
    }
}