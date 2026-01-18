namespace ModelLayer.Responses
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string[]>? Errors { get; set; }

        /// <summary>
        /// Constructor for simple error response
        /// </summary>
        public ErrorResponse(string message, string errorCode = "ERROR")
        {
            Message = message;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Constructor for validation error response with detailed errors
        /// </summary>
        public ErrorResponse(string message, string errorCode, Dictionary<string, string[]> errors)
        {
            Message = message;
            ErrorCode = errorCode;
            Errors = errors;
        }
    }
}
