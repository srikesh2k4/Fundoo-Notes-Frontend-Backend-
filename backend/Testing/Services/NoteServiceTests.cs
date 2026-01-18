// ========================================
// FILE: Testing/Services/NoteServiceTests.cs
// ========================================
using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using FluentAssertions;
using ModelLayer.DTOs.Notes;
using Moq;
using NUnit.Framework;

namespace Testing.Services
{
    [TestFixture]
    public class NoteServiceTests
    {
        private Mock<INoteRepository> _noteRepositoryMock = null!;
        private Mock<ICollaboratorRepository> _collaboratorRepositoryMock = null!;
        private NoteService _noteService = null!;

        [SetUp]
        public void Setup()
        {
            _noteRepositoryMock = new Mock<INoteRepository>();
            _collaboratorRepositoryMock = new Mock<ICollaboratorRepository>();
            _noteService = new NoteService(_noteRepositoryMock.Object, _collaboratorRepositoryMock.Object);
        }

        [Test]
        public async Task CreateAsync_ValidNote_ReturnsNoteResponse()
        {
            var dto = new CreateNoteDto
            {
                Title = "Test Note",
                Content = "Test Content",
                Color = "#FFFFFF"
            };
            var userId = 1;

            _noteRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Note>()))
                .Returns(Task.CompletedTask);

            _noteRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _noteService.CreateAsync(dto, userId);

            result.Should().NotBeNull();
            result.Title.Should().Be(dto.Title);
            result.Content.Should().Be(dto.Content);
            result.Color.Should().Be(dto.Color);
            _noteRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Note>()), Times.Once);
        }

        [Test]
        public async Task CreateAsync_EmptyTitleAndContent_ThrowsValidationException()
        {
            var dto = new CreateNoteDto { Title = "", Content = "" };
            var userId = 1;

            Func<Task> act = async () => await _noteService.CreateAsync(dto, userId);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Either title or content is required");
        }

        [Test]
        public async Task CreateAsync_InvalidColor_ThrowsValidationException()
        {
            var dto = new CreateNoteDto
            {
                Title = "Test",
                Color = "#GGGGGG"
            };
            var userId = 1;

            Func<Task> act = async () => await _noteService.CreateAsync(dto, userId);
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task GetByIdAsync_ValidNote_ReturnsNote()
        {
            var noteId = 1;
            var userId = 1;
            var note = new Note
            {
                Id = noteId,
                UserId = userId,
                Title = "Test",
                Content = "Content",
                Color = "#FFFFFF",
                IsDeleted = false
            };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            var result = await _noteService.GetByIdAsync(noteId, userId);

            result.Should().NotBeNull();
            result!.Id.Should().Be(noteId);
            result.Title.Should().Be(note.Title);
        }

        [Test]
        public async Task GetByIdAsync_UnauthorizedUser_ThrowsUnauthorizedException()
        {
            var noteId = 1;
            var ownerId = 1;
            var otherUserId = 2;
            var note = new Note
            {
                Id = noteId,
                UserId = ownerId,
                IsDeleted = false
            };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            _collaboratorRepositoryMock.Setup(x => x.ExistsAsync(noteId, otherUserId))
                .ReturnsAsync(false);

            Func<Task> act = async () => await _noteService.GetByIdAsync(noteId, otherUserId);
            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Test]
        public async Task UpdateAsync_ValidUpdate_ReturnsUpdatedNote()
        {
            var noteId = 1;
            var userId = 1;
            var note = new Note
            {
                Id = noteId,
                UserId = userId,
                Title = "Old Title",
                Content = "Old Content"
            };

            var dto = new UpdateNoteDto
            {
                Title = "New Title",
                Content = "New Content"
            };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            _noteRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _noteService.UpdateAsync(noteId, dto, userId);

            result.Should().NotBeNull();
            result.Title.Should().Be(dto.Title);
            result.Content.Should().Be(dto.Content);
        }

        [Test]
        public async Task UpdateAsync_NoteNotFound_ThrowsNotFoundException()
        {
            var noteId = 999;
            var userId = 1;
            var dto = new UpdateNoteDto { Title = "New" };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync((Note?)null);

            Func<Task> act = async () => await _noteService.UpdateAsync(noteId, dto, userId);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task DeleteAsync_ValidNote_SoftDeletesNote()
        {
            var noteId = 1;
            var userId = 1;
            var note = new Note
            {
                Id = noteId,
                UserId = userId,
                IsDeleted = false
            };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            _noteRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _noteService.DeleteAsync(noteId, userId);

            note.IsDeleted.Should().BeTrue();
            note.DeletedAt.Should().NotBeNull();
        }

        [Test]
        public async Task TogglePinAsync_ValidNote_TogglesPin()
        {
            var noteId = 1;
            var userId = 1;
            var note = new Note
            {
                Id = noteId,
                UserId = userId,
                IsPinned = false
            };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            _noteRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _noteService.TogglePinAsync(noteId, userId);

            result.IsPinned.Should().BeTrue();
            note.IsPinned.Should().BeTrue();
        }

        [Test]
        public async Task SearchAsync_ValidQuery_ReturnsMatchingNotes()
        {
            var userId = 1;
            var query = "test";
            var notes = new List<Note>
            {
                new Note { Id = 1, UserId = userId, Title = "Test Note 1" },
                new Note { Id = 2, UserId = userId, Content = "Contains test" }
            };

            var dto = new SearchNotesDto { Query = query };

            _noteRepositoryMock.Setup(x => x.SearchAsync(query, userId))
                .ReturnsAsync(notes);

            var result = await _noteService.SearchAsync(dto, userId);

            result.Should().HaveCount(2);
        }

        [Test]
        public async Task BulkDeleteAsync_ValidIds_DeletesMultipleNotes()
        {
            var userId = 1;
            var noteIds = new List<int> { 1, 2, 3 };
            var notes = noteIds.Select(id => new Note
            {
                Id = id,
                UserId = userId,
                IsDeleted = false
            }).ToList();

            var dto = new BulkDeleteDto { NoteIds = noteIds };

            _noteRepositoryMock.Setup(x => x.GetByIdsAsync(noteIds))
                .ReturnsAsync(notes);

            _noteRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _noteService.BulkDeleteAsync(dto, userId);

            notes.Should().AllSatisfy(n => n.IsDeleted.Should().BeTrue());
        }
    }
}
