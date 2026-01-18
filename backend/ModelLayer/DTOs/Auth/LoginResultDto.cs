namespace ModelLayer.DTOs.Auth
{
    public class LoginResultDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}