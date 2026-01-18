using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces.Services;
using FundooNotes.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs.Auth;
using ModelLayer.Responses;

namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly OtpEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            JwtTokenGenerator jwtTokenGenerator,
            OtpEmailSender emailSender,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailSender = emailSender;
            _logger = logger;
        }
        /// Register a new user

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", dto.Email);

            var otp = await _authService.RegisterAsync(dto);

            // Send OTP email
            await _emailSender.SendOtpEmailAsync(dto.Email, dto.Name, otp);

            _logger.LogInformation("User registered successfully. OTP sent to: {Email}", dto.Email);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse.SuccessResponse("Registration successful. Please verify your email with the OTP sent"));
        }


        /// Verify email with OTP
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto)
        {
            _logger.LogInformation("OTP verification attempt for email: {Email}", dto.Email);

            await _authService.VerifyOtpAsync(dto.Email, dto.Otp);

            _logger.LogInformation("Email verified successfully for: {Email}", dto.Email);

            return Ok(ApiResponse.SuccessResponse("Email verified successfully. You can now login"));
        }


        /// Login user
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var result = await _authService.LoginAsync(dto);

            // Generate JWT access token
            var accessToken = _jwtTokenGenerator.GenerateToken(result.UserId, result.Email);
            result.AccessToken = accessToken;

            _logger.LogInformation("User logged in successfully: {Email}", dto.Email);

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful"));
        }


        /// Refresh access token
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            _logger.LogInformation("Token refresh attempt");

            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

            // Generate new JWT access token
            var accessToken = _jwtTokenGenerator.GenerateToken(result.UserId, result.Email);
            result.AccessToken = accessToken;

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", result.UserId);

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Token refreshed successfully"));
        }


        /// Request password reset
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            _logger.LogInformation("Password reset requested for email: {Email}", dto.Email);

            await _authService.ForgotPasswordAsync(dto.Email);

            // Note: We always return success to prevent email enumeration
            return Ok(ApiResponse.SuccessResponse(
                "If the email exists, a password reset link has been sent"));
        }

        /// Reset password with token
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            _logger.LogInformation("Password reset attempt with token");

            await _authService.ResetPasswordAsync(dto);

            _logger.LogInformation("Password reset successfully");

            return Ok(ApiResponse.SuccessResponse("Password reset successfully. You can now login"));
        }


        /// Logout user
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserId();
            _logger.LogInformation("Logout attempt for user: {UserId}", userId);

            await _authService.LogoutAsync(userId);

            _logger.LogInformation("User logged out successfully: {UserId}", userId);

            return Ok(ApiResponse.SuccessResponse("Logged out successfully"));
        }

        /// Resend OTP for email verification
        [HttpPost("resend-otp")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendOtp([FromBody] ForgotPasswordDto dto)
        {
            _logger.LogInformation("Resend OTP request for email: {Email}", dto.Email);

            var otp = await _authService.ResendOtpAsync(dto.Email);

            // Send OTP email
            await _emailSender.SendOtpEmailAsync(dto.Email, "", otp);

            _logger.LogInformation("OTP resent successfully to: {Email}", dto.Email);

            return Ok(ApiResponse.SuccessResponse("OTP sent successfully. Please check your email"));
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}