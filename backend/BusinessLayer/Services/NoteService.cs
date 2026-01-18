using BusinessLayer.Exceptions;
using BusinessLayer.Interfaces.Services;
using BusinessLayer.Rules;
using DataBaseLayer.Entities;
using DataBaseLayer.Interfaces;
using ModelLayer.DTOs.Notes;

namespace BusinessLayer.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;
        private readonly ICollaboratorRepository _collaboratorRepository;

        public NoteService(INoteRepository noteRepository, ICollaboratorRepository collaboratorRepository)
        {
            _noteRepository = noteRepository;
            _collaboratorRepository = collaboratorRepository;
        }

        public async Task<IEnumerable<NoteResponseDto>> GetAllAsync(int userId)
        {
            var notes = await _noteRepository.GetByUserIdAsync(userId);
            return notes.Select(MapToDto);
        }

        public async Task<NoteResponseDto?> GetByIdAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId);

            if (note == null)
                return null;

            // Check if user is owner or collaborator
            if (note.UserId != userId && !await _collaboratorRepository.ExistsAsync(noteId, userId))
                throw new UnauthorizedException("Access denied to this note");

            return MapToDto(note);
        }

        public async Task<NoteResponseDto> CreateAsync(CreateNoteDto dto, int userId)
        {
            NoteRules.ValidateCreate(dto);

            var note = new Note
            {
                Title = dto.Title ?? "Untitled",
                Content = dto.Content ?? string.Empty,
                Color = dto.Color ?? "#FFFFFF",
                UserId = userId,
                IsPinned = false,
                IsArchived = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _noteRepository.AddAsync(note);
            await _noteRepository.SaveAsync();

            return MapToDto(note);
        }

        public async Task<NoteResponseDto> UpdateAsync(int noteId, UpdateNoteDto dto, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            // Check permission (owner or collaborator with edit permission)
            if (note.UserId != userId)
            {
                var collaborator = await _collaboratorRepository.GetByNoteAndUserAsync(noteId, userId);
                if (collaborator == null || collaborator.Permission != DataBaseLayer.Enums.PermissionLevel.Edit)
                    throw new UnauthorizedException("You don't have permission to edit this note");
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                if (dto.Title.Length > 200)
                    throw new ValidationException("Title cannot exceed 200 characters");
                note.Title = dto.Title;
            }

            if (dto.Content != null)
            {
                if (dto.Content.Length > 10000)
                    throw new ValidationException("Content cannot exceed 10000 characters");
                note.Content = dto.Content;
            }

            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();

            return MapToDto(note);
        }

        public async Task DeleteAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can delete this note");

            // Soft delete
            note.IsDeleted = true;
            note.DeletedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();
        }

        public async Task BulkDeleteAsync(BulkDeleteDto dto, int userId)
        {
            if (dto.NoteIds == null || !dto.NoteIds.Any())
                throw new ValidationException("At least one note ID is required");

            if (dto.NoteIds.Count() > 100)
                throw new ValidationException("Cannot delete more than 100 notes at once");

            var notes = await _noteRepository.GetByIdsAsync(dto.NoteIds);

            foreach (var note in notes)
            {
                if (note.UserId != userId)
                    continue; // Skip notes that don't belong to user

                note.IsDeleted = true;
                note.DeletedAt = DateTime.UtcNow;
            }

            await _noteRepository.SaveAsync();
        }

        public async Task<IEnumerable<NoteResponseDto>> SearchAsync(SearchNotesDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Query))
                return new List<NoteResponseDto>();

            if (dto.Query.Length > 200)
                throw new ValidationException("Search query cannot exceed 200 characters");

            var notes = await _noteRepository.SearchAsync(dto.Query, userId);
            return notes.Select(MapToDto);
        }

        public async Task<NoteResponseDto> TogglePinAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can pin/unpin notes");

            note.IsPinned = !note.IsPinned;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();

            return MapToDto(note);
        }

        public async Task<NoteResponseDto> ToggleArchiveAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can archive/unarchive notes");

            note.IsArchived = !note.IsArchived;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();

            return MapToDto(note);
        }

        public async Task<NoteResponseDto> UpdateColorAsync(int noteId, UpdateNoteColorDto dto, int userId)
        {
            NoteRules.ValidateColor(dto.Color);

            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can change note color");

            note.Color = dto.Color;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();

            return MapToDto(note);
        }

        public async Task<NoteResponseDto> ToggleTrashAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can trash/restore notes");

            note.IsDeleted = !note.IsDeleted;
            note.DeletedAt = note.IsDeleted ? DateTime.UtcNow : null;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();

            return MapToDto(note);
        }

        public async Task<IEnumerable<NoteResponseDto>> GetTrashedAsync(int userId)
        {
            var notes = await _noteRepository.GetTrashedByUserIdAsync(userId);
            return notes.Select(MapToDto);
        }

        public async Task RestoreFromTrashAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can restore this note");

            if (!note.IsDeleted)
                throw new ValidationException("Note is not in trash");

            note.IsDeleted = false;
            note.DeletedAt = null;
            note.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.SaveAsync();
        }

        public async Task DeletePermanentlyAsync(int noteId, int userId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId)
                ?? throw new NotFoundException("Note not found");

            if (note.UserId != userId)
                throw new UnauthorizedException("Only the owner can permanently delete this note");

            await _noteRepository.DeleteAsync(note);
            await _noteRepository.SaveAsync();
        }

        public async Task EmptyTrashAsync(int userId)
        {
            await _noteRepository.DeleteAllTrashedAsync(userId);
            await _noteRepository.SaveAsync();
        }

        private static NoteResponseDto MapToDto(Note note)
        {
            return new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title ?? "Untitled",
                Content = note.Content ?? string.Empty,
                Color = note.Color,
                IsPinned = note.IsPinned,
                IsArchived = note.IsArchived,
                IsDeleted = note.IsDeleted,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }
}