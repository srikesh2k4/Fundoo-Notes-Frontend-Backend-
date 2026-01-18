// ========================================
// FILE: Testing/Integration/LabelControllerIntegrationTests.cs
// ========================================
using FluentAssertions;
using ModelLayer.DTOs.Labels;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Testing.Integration
{
    [TestFixture]
    public class LabelControllerIntegrationTests
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
        public async Task CreateLabel_WithoutAuth_ReturnsUnauthorized()
        {
            var createDto = new CreateLabelDto { Name = "Work" };

            var response = await _client.PostAsJsonAsync("/api/labels", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetAllLabels_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/labels");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetLabelById_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/labels/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateLabel_WithoutAuth_ReturnsUnauthorized()
        {
            var updateDto = new UpdateLabelDto { Name = "Updated" };

            var response = await _client.PutAsJsonAsync("/api/labels/1", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteLabel_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.DeleteAsync("/api/labels/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetLabelById_NonExistent_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/labels/999");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task CreateLabel_EmptyName_ReturnsUnauthorized()
        {
            var createDto = new CreateLabelDto { Name = "" };

            var response = await _client.PostAsJsonAsync("/api/labels", createDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdateLabel_EmptyName_ReturnsUnauthorized()
        {
            var updateDto = new UpdateLabelDto { Name = "" };

            var response = await _client.PutAsJsonAsync("/api/labels/1", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
