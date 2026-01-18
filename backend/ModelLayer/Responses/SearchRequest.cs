namespace ModelLayer.Requests
{
    public class SearchRequest : PaginationRequest
    {
        public string? Query { get; set; }
        public bool? IsPinned { get; set; }
        public bool? IsArchived { get; set; }
        public string? Color { get; set; }
        public List<int>? LabelIds { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
