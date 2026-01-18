// ========================================
// FILE: Testing/Services/CollaboratorServiceTests.cs (COMPLETE)
// ========================================
using BusinessLayer.Exceptions;
using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using FluentAssertions;
using ModelLayer.DTOs.Collaborators;
using ModelLayer.Enums;
using Moq;
using NUnit.Framework;

namespace Testing.Services
{
    [TestFixture]
    public class CollaboratorServiceTests
    {
        private Mock<ICollaboratorRepository> _collaboratorRepositoryMock = null!;
        private Mock<INoteRepository> _noteRepositoryMock = null!;
        private Mock<IUserRepository> _userRepositoryMock = null!;
        private CollaboratorService _collaboratorService = null!;

        [SetUp]
        public void Setup()
        {
            _collaboratorRepositoryMock = new Mock<ICollaboratorRepository>();
            _noteRepositoryMock = new Mock<INoteRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _collaboratorService = new CollaboratorService(
                _collaboratorRepositoryMock.Object,
                _noteRepositoryMock.Object,
                _userRepositoryMock.Object);
        }

        [Test]
        public async Task AddAsync_ValidCollaborator_ReturnsCollaboratorResponse()
        {
            var dto = new AddCollaboratorDto
            {
                NoteId = 1,
                UserId = 2,
                Permission = PermissionLevel.Edit
            };
            var ownerUserId = 1;

            var note = new Note { Id = 1, UserId = ownerUserId };
            var collaboratorUser = new User { Id = 2, Email = "collaborator@example.com" };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(dto.NoteId))
                .ReturnsAsync(note);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(dto.UserId))
                .ReturnsAsync(collaboratorUser);

            _collaboratorRepositoryMock.Setup(x => x.ExistsAsync(dto.NoteId, dto.UserId))
                .ReturnsAsync(false);

            _collaboratorRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Collaborator>()))
                .Returns(Task.CompletedTask);

            _collaboratorRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _collaboratorService.AddAsync(dto, ownerUserId);

            result.Should().NotBeNull();
            result.NoteId.Should().Be(dto.NoteId);
            result.UserId.Should().Be(dto.UserId);
            result.UserEmail.Should().Be(collaboratorUser.Email);
            result.Permission.Should().Be(dto.Permission);
        }

        [Test]
        public async Task AddAsync_NoteNotFound_ThrowsNotFoundException()
        {
            var dto = new AddCollaboratorDto { NoteId = 999, UserId = 2 };
            var ownerUserId = 1;

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(dto.NoteId))
                .ReturnsAsync((Note?)null);

            Func<Task> act = async () => await _collaboratorService.AddAsync(dto, ownerUserId);
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Note not found");
        }

        [Test]
        public async Task AddAsync_NotOwner_ThrowsUnauthorizedException()
        {
            var dto = new AddCollaboratorDto { NoteId = 1, UserId = 2 };
            var ownerUserId = 1;
            var otherUserId = 3;

            var note = new Note { Id = 1, UserId = ownerUserId };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(dto.NoteId))
                .ReturnsAsync(note);

            Func<Task> act = async () => await _collaboratorService.AddAsync(dto, otherUserId);
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Only the note owner can add collaborators");
        }

        [Test]
        public async Task AddAsync_AddSelfAsCollaborator_ThrowsValidationException()
        {
            var dto = new AddCollaboratorDto { NoteId = 1, UserId = 1 };
            var ownerUserId = 1;

            var note = new Note { Id = 1, UserId = ownerUserId };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(dto.NoteId))
                .ReturnsAsync(note);

            Func<Task> act = async () => await _collaboratorService.AddAsync(dto, ownerUserId);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Cannot add yourself as a collaborator");
        }

        [Test]
        public async Task AddAsync_CollaboratorUserNotFound_ThrowsNotFoundException()
        {
            var dto = new AddCollaboratorDto { NoteId = 1, UserId = 999 };
            var ownerUserId = 1;

            var note = new Note { Id = 1, UserId = ownerUserId };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(dto.NoteId))
                .ReturnsAsync(note);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(dto.UserId))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _collaboratorService.AddAsync(dto, ownerUserId);
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Collaborator user not found");
        }

        [Test]
        public async Task AddAsync_DuplicateCollaborator_ThrowsValidationException()
        {
            var dto = new AddCollaboratorDto { NoteId = 1, UserId = 2 };
            var ownerUserId = 1;

            var note = new Note { Id = 1, UserId = ownerUserId };
            var collaboratorUser = new User { Id = 2, Email = "collaborator@example.com" };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(dto.NoteId))
                .ReturnsAsync(note);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(dto.UserId))
                .ReturnsAsync(collaboratorUser);

            _collaboratorRepositoryMock.Setup(x => x.ExistsAsync(dto.NoteId, dto.UserId))
                .ReturnsAsync(true);

            Func<Task> act = async () => await _collaboratorService.AddAsync(dto, ownerUserId);
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("This user is already a collaborator on this note");
        }

        [Test]
        public async Task GetByNoteIdAsync_ValidNote_ReturnsCollaborators()
        {
            var noteId = 1;
            var userId = 1;
            var note = new Note { Id = noteId, UserId = userId };

            var collaborators = new List<Collaborator>
            {
                new Collaborator
                {
                    Id = 1,
                    NoteId = noteId,
                    CollaboratorId = 2,
                    CollaboratorUser = new User { Id = 2, Email = "user2@example.com" },
                    Permission = DataBaseLayer.Enums.PermissionLevel.View,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            _collaboratorRepositoryMock.Setup(x => x.GetByNoteIdAsync(noteId))
                .ReturnsAsync(collaborators);

            var result = await _collaboratorService.GetByNoteIdAsync(noteId, userId);

            result.Should().HaveCount(1);
            result.First().UserEmail.Should().Be("user2@example.com");
        }

        [Test]
        public async Task GetByNoteIdAsync_NotOwnerOrCollaborator_ThrowsUnauthorizedException()
        {
            var noteId = 1;
            var ownerId = 1;
            var otherUserId = 3;

            var note = new Note { Id = noteId, UserId = ownerId };

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(noteId))
                .ReturnsAsync(note);

            _collaboratorRepositoryMock.Setup(x => x.ExistsAsync(noteId, otherUserId))
                .ReturnsAsync(false);

            Func<Task> act = async () => await _collaboratorService.GetByNoteIdAsync(noteId, otherUserId);
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Access denied");
        }

        [Test]
        public async Task UpdatePermissionAsync_ValidUpdate_ReturnsUpdatedCollaborator()
        {
            var collaboratorId = 1;
            var ownerUserId = 1;
            var dto = new UpdatePermissionDto { Permission = PermissionLevel.Edit };

            var collaborator = new Collaborator
            {
                Id = collaboratorId,
                NoteId = 1,
                CollaboratorId = 2,
                CollaboratorUser = new User { Email = "collaborator@example.com" },
                Permission = DataBaseLayer.Enums.PermissionLevel.View,
                CreatedAt = DateTime.UtcNow
            };

            var note = new Note { Id = 1, UserId = ownerUserId };

            _collaboratorRepositoryMock.Setup(x => x.GetByIdWithUserAsync(collaboratorId))
                .ReturnsAsync(collaborator);

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(collaborator.NoteId))
                .ReturnsAsync(note);

            _collaboratorRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            var result = await _collaboratorService.UpdatePermissionAsync(collaboratorId, dto, ownerUserId);

            result.Should().NotBeNull();
            result.Permission.Should().Be(dto.Permission);
            collaborator.Permission.Should().Be(DataBaseLayer.Enums.PermissionLevel.Edit);
        }

        [Test]
        public async Task UpdatePermissionAsync_NotOwner_ThrowsUnauthorizedException()
        {
            var collaboratorId = 1;
            var ownerUserId = 1;
            var otherUserId = 3;
            var dto = new UpdatePermissionDto { Permission = PermissionLevel.Edit };

            var collaborator = new Collaborator
            {
                Id = collaboratorId,
                NoteId = 1,
                CollaboratorId = 2
            };

            var note = new Note { Id = 1, UserId = ownerUserId };

            _collaboratorRepositoryMock.Setup(x => x.GetByIdWithUserAsync(collaboratorId))
                .ReturnsAsync(collaborator);

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(collaborator.NoteId))
                .ReturnsAsync(note);

            Func<Task> act = async () => await _collaboratorService.UpdatePermissionAsync(collaboratorId, dto, otherUserId);
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Only the note owner can update permissions");
        }

        [Test]
        public async Task RemoveAsync_ValidCollaborator_RemovesCollaborator()
        {
            var collaboratorId = 1;
            var ownerUserId = 1;

            var collaborator = new Collaborator
            {
                Id = collaboratorId,
                NoteId = 1,
                CollaboratorId = 2
            };

            var note = new Note { Id = 1, UserId = ownerUserId };

            _collaboratorRepositoryMock.Setup(x => x.GetByIdAsync(collaboratorId))
                .ReturnsAsync(collaborator);

            _noteRepositoryMock.Setup(x => x.GetByIdAsync(collaborator.NoteId))
                .ReturnsAsync(note);

            _collaboratorRepositoryMock.Setup(x => x.DeleteAsync(collaborator))
                .Returns(Task.CompletedTask);

            _collaboratorRepositoryMock.Setup(x => x.SaveAsync())
                .Returns(Task.CompletedTask);

            await _collaboratorService.RemoveAsync(collaboratorId, ownerUserId);

            _collaboratorRepositoryMock.Verify(x => x.DeleteAsync(collaborator), Times.Once);
            _collaboratorRepositoryMock.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task RemoveAsync_CollaboratorNotFound_ThrowsNotFoundException()
        {
            var collaboratorId = 999;
            var ownerUserId = 1;

            _collaboratorRepositoryMock.Setup(x => x.GetByIdAsync(collaboratorId))
                .ReturnsAsync((Collaborator?)null);

            Func<Task> act = async () => await _collaboratorService.RemoveAsync(collaboratorId, ownerUserId);
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Collaborator not found");
        }
    }
}
