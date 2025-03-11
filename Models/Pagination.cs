namespace TutorialIdentity.Models {
    public class Pagination {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public Func<int?, int?, string> GetPageUrl { get; set; } = (page, pageSize) => $"?p={page}&ps={pageSize}";
    }

}
