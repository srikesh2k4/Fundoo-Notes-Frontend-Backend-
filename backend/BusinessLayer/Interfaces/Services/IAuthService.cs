using ModelLayer.DTOs.Auth;

namespace BusinessLayer.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto dto);
        Task VerifyOtpAsync(string email, string otp);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task LogoutAsync(int userId);
        Task<string> ResendOtpAsync(string email);
    }
}