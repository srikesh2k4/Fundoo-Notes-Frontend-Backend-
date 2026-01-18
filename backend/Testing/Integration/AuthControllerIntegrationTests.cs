// ========================================
// FILE: Testing/Integration/AuthControllerIntegrationTests.cs
// ========================================
using FluentAssertions;
using ModelLayer.DTOs.Auth;
using ModelLayer.Responses;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Testing.Integration
{
    [TestFixture]
    public class AuthControllerIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Test]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            var registerDto = new RegisterRequestDto
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Password = "SecurePass123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            result.Should().NotBeNull();
            result!.Success.Should().BeTrue();
            result.Message.Should().Contain("Registration successful");
        }

        [Test]
        public async Task Register_DuplicateEmail_ReturnsBadRequest()
        {
            var registerDto = new RegisterRequestDto
            {
                Name = "Jane Doe",
                Email = "duplicate@example.com",
                Password = "SecurePass123"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDto = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/auth/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
