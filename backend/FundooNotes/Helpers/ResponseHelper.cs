using ModelLayer.Responses;

namespace FundooNotes.Helpers
{
    public static class ResponseHelper
    {
        public static ApiResponse<T> Success<T>(T data, string message = "Success")
        {
            return ApiResponse<T>.SuccessResponse(data, message);
        }

        public static ApiResponse Success(string message = "Success")
        {
            return ApiResponse.SuccessResponse(message);
        }

        public static ErrorResponse Error(string message, string? errorCode = null)
        {
            return new ErrorResponse(message, errorCode ?? "ERROR");
        }

        public static ErrorResponse ValidationError(string message, Dictionary<string, string[]> errors)
        {
            return new ErrorResponse(message, "VALIDATION_ERROR", errors);  // ✅ Fixed!
        }
    }
}
