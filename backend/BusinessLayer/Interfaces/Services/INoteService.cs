using ModelLayer.DTOs.Notes;

namespace BusinessLayer.Interfaces.Services
{
    public interface INoteService
    {
        Task<IEnumerable<NoteResponseDto>> GetAllAsync(int userId);
        Task<NoteResponseDto?> GetByIdAsync(int noteId, int userId);
        Task<NoteResponseDto> CreateAsync(CreateNoteDto dto, int userId);
        Task<NoteResponseDto> UpdateAsync(int noteId, UpdateNoteDto dto, int userId);
        Task DeleteAsync(int noteId, int userId);
        Task BulkDeleteAsync(BulkDeleteDto dto, int userId);
        Task<IEnumerable<NoteResponseDto>> SearchAsync(SearchNotesDto dto, int userId);
        Task<NoteResponseDto> TogglePinAsync(int noteId, int userId);
        Task<NoteResponseDto> ToggleArchiveAsync(int noteId, int userId);
        Task<NoteResponseDto> ToggleTrashAsync(int noteId, int userId);
        Task<NoteResponseDto> UpdateColorAsync(int noteId, UpdateNoteColorDto dto, int userId);
        Task<IEnumerable<NoteResponseDto>> GetTrashedAsync(int userId);
        Task RestoreFromTrashAsync(int noteId, int userId);
        Task DeletePermanentlyAsync(int noteId, int userId);
        Task EmptyTrashAsync(int userId);
    }
}