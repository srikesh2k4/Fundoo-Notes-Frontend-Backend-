using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using ModelLayer.DTOs.Collaborators;

namespace BusinessLayer.Services
{
    public class CollaboratorService : ICollaboratorService
    {
        private readonly ICollaboratorRepository _collaboratorRepository;
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;

        public CollaboratorService(
            ICollaboratorRepository collaboratorRepository,
            INoteRepository noteRepository,
            IUserRepository userRepository)
        {
            _collaboratorRepository = collaboratorRepository;
            _noteRepository = noteRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<CollaboratorResponseDto>> GetByNoteIdAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            // Only owner and collaborators can view collaborators list
            if (note.UserId != userId && !await _collaboratorRepository.ExistsAsync(noteId, userId))
                throw new UnauthorizedException("Access denied");

            var collaborators = await _collaboratorRepository.GetByNoteIdAsync(noteId);

            return collaborators.Select(c => new CollaboratorResponseDto
            {
                Id = c.Id,
                NoteId = c.NoteId,
                UserId = c.CollaboratorId,
                UserEmail = c.CollaboratorUser?.Email ?? string.Empty,
                Permission = (ModelLayer.Enums.PermissionLevel)c.Permission,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CollaboratorResponseDto> AddAsync(AddCollaboratorDto dto, int ownerUserId)
        {
            var note = await _noteRepository.GetByIdAsync(dto.NoteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != ownerUserId)
                throw new UnauthorizedException("Only the note owner can add collaborators");

            if (dto.UserId == ownerUserId)
                throw new ValidationException("Cannot add yourself as a collaborator");

            // Check if collaborator user exists
            var collaboratorUser = await _userRepository.GetByIdAsync(dto.UserId)
                ?? throw new NotFoundException("Collaborator user not found");

            if (await _collaboratorRepository.ExistsAsync(dto.NoteId, dto.UserId))
                throw new ValidationException("This user is already a collaborator on this note");

            var collaborator = new Collaborator
            {
                NoteId = dto.NoteId,
                CollaboratorId = dto.UserId,
                Permission = (DataBaseLayer.Enums.PermissionLevel)dto.Permission,
                CreatedAt = DateTime.UtcNow
            };

            await _collaboratorRepository.AddAsync(collaborator);
            await _collaboratorRepository.SaveAsync();

            return new CollaboratorResponseDto
            {
                Id = collaborator.Id,
                NoteId = collaborator.NoteId,
                UserId = collaborator.CollaboratorId,
                UserEmail = collaboratorUser.Email,
                Permission = (ModelLayer.Enums.PermissionLevel)collaborator.Permission,
                CreatedAt = collaborator.CreatedAt
            };
        }

        public async Task<CollaboratorResponseDto> UpdatePermissionAsync(int collaboratorId, UpdatePermissionDto dto, int ownerUserId)
        {
            var collaborator = await _collaboratorRepository.GetByIdWithUserAsync(collaboratorId)
                ?? throw new NotFoundException("Collaborator not found");

            var note = await _noteRepository.GetByIdAsync(collaborator.NoteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != ownerUserId)
                throw new UnauthorizedException("Only the note owner can update permissions");

            collaborator.Permission = (DataBaseLayer.Enums.PermissionLevel)dto.Permission;
            collaborator.UpdatedAt = DateTime.UtcNow;

            await _collaboratorRepository.SaveAsync();

            return new CollaboratorResponseDto
            {
                Id = collaborator.Id,
                NoteId = collaborator.NoteId,
                UserId = collaborator.CollaboratorId,
                UserEmail = collaborator.CollaboratorUser?.Email ?? string.Empty,
                Permission = (ModelLayer.Enums.PermissionLevel)collaborator.Permission,
                CreatedAt = collaborator.CreatedAt
            };
        }

        public async Task RemoveAsync(int collaboratorId, int ownerUserId)
        {
            var collaborator = await _collaboratorRepository.GetByIdAsync(collaboratorId)
                ?? throw new NotFoundException("Collaborator not found");

            var note = await _noteRepository.GetByIdAsync(collaborator.NoteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != ownerUserId)
                throw new UnauthorizedException("Only the note owner can remove collaborators");

            await _collaboratorRepository.DeleteAsync(collaborator);
            await _collaboratorRepository.SaveAsync();
        }
    }
}