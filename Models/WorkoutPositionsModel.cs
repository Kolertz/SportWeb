namespace SportWeb.Models
{
    public class WorkoutPositionsModel
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public bool IsSuperset { get; set; } = false;
        public int? SupersetId { get; set; }
    }
}
