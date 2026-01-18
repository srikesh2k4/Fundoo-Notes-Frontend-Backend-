// ========================================
// FILE: Testing/Services/AuthServiceTests.cs
// ========================================
using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using FluentAssertions;
using ModelLayer.DTOs.Auth;
using Moq;
using NUnit.Framework;

namespace Testing.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authService = new AuthService(_userRepositoryMock.Object);
        }

        [Test]
        public async Task RegisterAsync_ValidUser_ReturnsOtp()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "Test123"
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _authService.RegisterAsync(dto);

            result.Should().NotBeNullOrEmpty();
            result.Should().HaveLength(6);
            result.Should().MatchRegex(@"^\d{6}$");
            _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task RegisterAsync_DuplicateEmail_ThrowsValidationException()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Test User",
                Email = "existing@example.com",
                Password = "Test123"
            };

            var existingUser = new User { Id = 1, Email = dto.Email };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            Func<Task> act = async () => await _authService.RegisterAsync(dto);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("User with this email already exists");
        }

        [Test]
        public async Task RegisterAsync_InvalidName_ThrowsValidationException()
        {
            var dto = new RegisterRequestDto
            {
                Name = "A",
                Email = "test@example.com",
                Password = "Test123"
            };

            Func<Task> act = async () => await _authService.RegisterAsync(dto);
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task RegisterAsync_InvalidPassword_ThrowsValidationException()
        {
            var dto = new RegisterRequestDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Password = "weak"
            };

            Func<Task> act = async () => await _authService.RegisterAsync(dto);
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task VerifyOtpAsync_ValidOtp_VerifiesEmail()
        {
            var email = "test@example.com";
            var otp = "123456";
            var user = new User
            {
                Id = 1,
                Email = email,
                EmailVerificationToken = otp,
                EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(5),
                IsEmailVerified = false
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _authService.VerifyOtpAsync(email, otp);

            user.IsEmailVerified.Should().BeTrue();
            user.EmailVerificationToken.Should().BeNull();
            user.EmailVerificationExpiry.Should().BeNull();
            _userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task VerifyOtpAsync_InvalidOtp_ThrowsValidationException()
        {
            var email = "test@example.com";
            var user = new User
            {
                Email = email,
                EmailVerificationToken = "123456",
                EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(5)
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.VerifyOtpAsync(email, "999999");
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Invalid OTP");
        }

        [Test]
        public async Task VerifyOtpAsync_ExpiredOtp_ThrowsValidationException()
        {
            var email = "test@example.com";
            var user = new User
            {
                Email = email,
                EmailVerificationToken = "123456",
                EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(-5)
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.VerifyOtpAsync(email, "123456");
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("OTP has expired");
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Test123"
            };

            var user = new User
            {
                Id = 1,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsEmailVerified = true,
                FailedLoginAttempts = 0
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _authService.LoginAsync(dto);

            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
            result.RefreshToken.Should().NotBeNullOrEmpty();
            user.FailedLoginAttempts.Should().Be(0);
        }

        [Test]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
        {
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123"),
                IsEmailVerified = true
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(dto);
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Invalid credentials");
        }

        [Test]
        public async Task LoginAsync_UnverifiedEmail_ThrowsUnauthorizedException()
        {
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Test123"
            };

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsEmailVerified = false
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(dto);
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Email not verified. Please verify your email first");
        }

        [Test]
        public async Task LoginAsync_AccountLocked_ThrowsUnauthorizedException()
        {
            var dto = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Test123"
            };

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsEmailVerified = true,
                LockoutEnd = DateTime.UtcNow.AddMinutes(10)
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(dto.Email))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.LoginAsync(dto);
            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Test]
        public async Task RefreshTokenAsync_ValidToken_ReturnsNewAuthResponse()
        {
            var refreshToken = "valid-refresh-token";
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };

            _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _authService.RefreshTokenAsync(refreshToken);

            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBe(refreshToken);
        }

        [Test]
        public async Task RefreshTokenAsync_ExpiredToken_ThrowsUnauthorizedException()
        {
            var refreshToken = "expired-refresh-token";
            var user = new User
            {
                RefreshToken = refreshToken,
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1)
            };

            _userRepositoryMock.Setup(x => x.GetByRefreshTokenAsync(refreshToken))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.RefreshTokenAsync(refreshToken);
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Refresh token has expired");
        }

        [Test]
        public async Task LogoutAsync_ValidUser_ClearsRefreshToken()
        {
            var userId = 1;
            var user = new User
            {
                Id = userId,
                RefreshToken = "some-token",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
            };

            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _authService.LogoutAsync(userId);

            user.RefreshToken.Should().BeNull();
            user.RefreshTokenExpiry.Should().BeNull();
            _userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ForgotPasswordAsync_ValidEmail_GeneratesResetToken()
        {
            var email = "test@example.com";
            var user = new User
            {
                Id = 1,
                Email = email
            };

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _authService.ForgotPasswordAsync(email);

            user.PasswordResetToken.Should().NotBeNullOrEmpty();
            user.PasswordResetExpiry.Should().NotBeNull();
            _userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ForgotPasswordAsync_NonExistentEmail_DoesNotThrow()
        {
            var email = "nonexistent@example.com";

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _authService.ForgotPasswordAsync(email);

            await act.Should().NotThrowAsync();
        }

        [Test]
        public async Task ResetPasswordAsync_ValidToken_ResetsPassword()
        {
            var token = "valid-reset-token";
            var newPassword = "NewSecure123";
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordResetToken = token,
                PasswordResetExpiry = DateTime.UtcNow.AddMinutes(15),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword123")
            };

            var dto = new ResetPasswordDto
            {
                Token = token,
                NewPassword = newPassword
            };

            _userRepositoryMock.Setup(x => x.GetByResetTokenAsync(token))
                .ReturnsAsync(user);

            _userRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _authService.ResetPasswordAsync(dto);

            user.PasswordResetToken.Should().BeNull();
            user.PasswordResetExpiry.Should().BeNull();
            BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash).Should().BeTrue();
            _userRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ResetPasswordAsync_ExpiredToken_ThrowsValidationException()
        {
            var token = "expired-token";
            var user = new User
            {
                PasswordResetToken = token,
                PasswordResetExpiry = DateTime.UtcNow.AddMinutes(-15)
            };

            var dto = new ResetPasswordDto
            {
                Token = token,
                NewPassword = "NewSecure123"
            };

            _userRepositoryMock.Setup(x => x.GetByResetTokenAsync(token))
                .ReturnsAsync(user);

            Func<Task> act = async () => await _authService.ResetPasswordAsync(dto);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Password reset token has expired");
        }
    }
}
