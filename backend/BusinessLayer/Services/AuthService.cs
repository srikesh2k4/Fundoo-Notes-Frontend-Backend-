// ========================================
// FILE: BusinessLayer/Services/AuthService.cs (FIXED)
// ========================================
using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces.Services;
using BusinessLayer.Rules;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using ModelLayer.DTOs.Auth;
using System.Security.Cryptography;

namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<string> RegisterAsync(RegisterRequestDto dto)
        {
            // Validate input
            UserRules.ValidateRegistration(dto);

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ValidationException("User with this email already exists");

            // Generate OTP
            var otp = GenerateOtp();

            // Create new user
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerificationToken = otp,
                EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(10),
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            return otp;
        }

        public async Task VerifyOtpAsync(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email is required");

            if (string.IsNullOrWhiteSpace(otp))
                throw new ValidationException("OTP is required");

            var user = await _userRepository.GetByEmailAsync(email)
                ?? throw new NotFoundException("User not found");

            if (user.EmailVerificationToken != otp)
                throw new ValidationException("Invalid OTP");

            if (user.EmailVerificationExpiry < DateTime.UtcNow)
                throw new ValidationException("OTP has expired");

            // Verify email
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;

            await _userRepository.SaveAsync();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ValidationException("Email is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ValidationException("Password is required");

            var user = await _userRepository.GetByEmailAsync(dto.Email)
                ?? throw new UnauthorizedException("Invalid credentials");

            // Check account lockout
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var remainingTime = (user.LockoutEnd.Value - DateTime.UtcNow).Minutes;
                throw new UnauthorizedException($"Account is locked. Try again in {remainingTime} minutes");
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                    await _userRepository.SaveAsync();
                    throw new UnauthorizedException("Too many failed login attempts. Account locked for 15 minutes");
                }

                await _userRepository.SaveAsync();
                throw new UnauthorizedException("Invalid credentials");
            }

            // Check if email is verified
            if (!user.IsEmailVerified)
                throw new UnauthorizedException("Email not verified. Please verify your email first");

            // Reset failed login attempts
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userRepository.SaveAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                RefreshToken = refreshToken,
                ExpiresIn = 3600
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ValidationException("Refresh token is required");

            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken)
                ?? throw new UnauthorizedException("Invalid refresh token");

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token has expired");

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _userRepository.SaveAsync();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            };
        }

        public async Task ForgotPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email is required");

            var user = await _userRepository.GetByEmailAsync(email);

            // Don't reveal if email exists (security best practice)
            if (user == null)
                return;

            // Generate reset token
            var resetToken = GenerateResetToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userRepository.SaveAsync();

            // In production, send email with reset link containing token
            // await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                throw new ValidationException("Reset token is required");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new ValidationException("New password is required");

            // Validate new password
            UserRules.ValidatePassword(dto.NewPassword);

            var user = await _userRepository.GetByResetTokenAsync(dto.Token)
                ?? throw new ValidationException("Invalid or expired reset token");

            if (user.PasswordResetExpiry < DateTime.UtcNow)
                throw new ValidationException("Password reset token has expired");

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;

            // Invalidate all refresh tokens for security
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _userRepository.SaveAsync();
        }

        public async Task LogoutAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            // Invalidate refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _userRepository.SaveAsync();
        }

        public async Task<string> ResendOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Email is required");

            var user = await _userRepository.GetByEmailAsync(email)
                ?? throw new NotFoundException("User not found");

            if (user.IsEmailVerified)
                throw new ValidationException("Email is already verified");

            // Generate new OTP
            var otp = GenerateOtp();
            user.EmailVerificationToken = otp;
            user.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.SaveAsync();

            return otp;
        }

        // Helper method to generate 6-digit OTP
        private static string GenerateOtp()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        // Helper method to generate secure refresh token
        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Helper method to generate password reset token
        private static string GenerateResetToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}