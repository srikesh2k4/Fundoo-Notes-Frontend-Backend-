using System.Security.Claims;

namespace FundooNotes.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID format");
            }

            return userId;
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                throw new UnauthorizedAccessException("Email not found in token");
            }

            return email;
        }

        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return principal.Identity?.IsAuthenticated ?? false;
        }
    }
}