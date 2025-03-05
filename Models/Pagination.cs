namespace TutorialIdentity.Models {
    public class Pagination {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public Func<int?, string> GetPageUrl { get; set; } = page => $"?p={page}";
    }
}
