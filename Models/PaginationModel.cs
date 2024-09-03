namespace SportWeb.Models
{
    public class PaginationModel
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public int StartPage => Math.Max(1, CurrentPage - 5);
        public int EndPage => Math.Min(TotalPages, CurrentPage + 4);
    }
}