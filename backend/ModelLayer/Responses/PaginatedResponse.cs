namespace ModelLayer.Responses
{
    public class PaginatedResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<T>? Data { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public DateTime Timestamp { get; set; }

        public PaginatedResponse()
        {
            Timestamp = DateTime.UtcNow;
        }

        public static PaginatedResponse<T> Create(
            IEnumerable<T> data,
            int totalCount,
            int pageNumber,
            int pageSize,
            string message = "Success")
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }
    }
}