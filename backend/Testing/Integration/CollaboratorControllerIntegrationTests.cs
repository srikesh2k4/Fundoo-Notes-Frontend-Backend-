// ========================================
// FILE: Testing/Integration/CollaboratorControllerIntegrationTests.cs
// ========================================
using FluentAssertions;
using ModelLayer.DTOs.Collaborators;
using ModelLayer.Enums;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace Testing.Integration
{
    [TestFixture]
    public class CollaboratorControllerIntegrationTests
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
        public async Task AddCollaborator_WithoutAuth_ReturnsUnauthorized()
        {
            var addDto = new AddCollaboratorDto
            {
                NoteId = 1,
                UserId = 2,
                Permission = PermissionLevel.View
            };

            var response = await _client.PostAsJsonAsync("/api/collaborators", addDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetCollaborators_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/collaborators/note/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdatePermission_WithoutAuth_ReturnsUnauthorized()
        {
            var updateDto = new UpdatePermissionDto { Permission = PermissionLevel.Edit };

            var response = await _client.PutAsJsonAsync("/api/collaborators/1", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task RemoveCollaborator_WithoutAuth_ReturnsUnauthorized()
        {
            var response = await _client.DeleteAsync("/api/collaborators/1");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task AddCollaborator_InvalidNoteId_ReturnsUnauthorized()
        {
            var addDto = new AddCollaboratorDto
            {
                NoteId = 0,
                UserId = 2,
                Permission = PermissionLevel.View
            };

            var response = await _client.PostAsJsonAsync("/api/collaborators", addDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task AddCollaborator_InvalidUserId_ReturnsUnauthorized()
        {
            var addDto = new AddCollaboratorDto
            {
                NoteId = 1,
                UserId = 0,
                Permission = PermissionLevel.View
            };

            var response = await _client.PostAsJsonAsync("/api/collaborators", addDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetCollaborators_InvalidNoteId_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/collaborators/note/999");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task UpdatePermission_InvalidCollaboratorId_ReturnsUnauthorized()
        {
            var updateDto = new UpdatePermissionDto { Permission = PermissionLevel.Edit };

            var response = await _client.PutAsJsonAsync("/api/collaborators/999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task RemoveCollaborator_InvalidCollaboratorId_ReturnsUnauthorized()
        {
            var response = await _client.DeleteAsync("/api/collaborators/999");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
