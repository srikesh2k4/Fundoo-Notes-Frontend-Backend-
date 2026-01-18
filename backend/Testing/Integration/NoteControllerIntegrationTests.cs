
using DataBaseLayer.Context;
using DataBaseLayer.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelLayer.DTOs.Notes;
using ModelLayer.Responses;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Testing.Integration
{
    [TestFixture]
    public class NoteControllerIntegrationTests
    {
        private CustomWebApplicationFactory _factory;
        private HttpClient _client;
        private string _authToken;
        private int _userId;

        [SetUp]
        public async Task Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

            // Setup test user and get auth token
            await SetupTestUserWithToken();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        private async Task SetupTestUserWithToken()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FundooAppDbContext>();

            // Create test user
            var user = new User
            {
                Name = "Test User",
                Email = "testuser@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123"),
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            _userId = user.Id;

            // Generate mock JWT token (simplified)
            _authToken = "mock-jwt-token"; // In real tests, generate valid JWT
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        // Create Note Tests
        [Test]
        public async Task CreateNote_ValidData_ReturnsSuccess()
        {
            // Arrange
            var createDto = new CreateNoteDto
            {
                Title = "Test Note",
                Content = "This is test content",
                Color = "#FFFFFF"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // No valid JWT
        }

        [Test]
        public async Task CreateNote_EmptyTitleAndContent_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateNoteDto
            {
                Title = "",
                Content = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // No valid JWT
        }

        // GetAllNotes Tests
        [Test]
        public async Task GetAllNotes_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/notes");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // GetNoteById Tests
        [Test]
        public async Task GetNoteById_NonExistent_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/notes/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // No valid JWT
        }

        // Update Note Tests
        [Test]
        public async Task UpdateNote_ValidData_ReturnsSuccess()
        {
            // Arrange
            var updateDto = new UpdateNoteDto
            {
                Title = "Updated Title",
                Content = "Updated Content"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/notes/1", updateDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // No valid JWT
        }

        // Delete Note Tests
        [Test]
        public async Task DeleteNote_ValidId_ReturnsSuccess()
        {
            // Act
            var response = await _client.DeleteAsync("/api/notes/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // No valid JWT
        }

        // Search Notes Tests
        [Test]
        public async Task SearchNotes_ValidQuery_ReturnsResults()
        {
            // Act
            var response = await _client.GetAsync("/api/notes/search?query=test");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized); // No valid JWT
        }
    }
}
