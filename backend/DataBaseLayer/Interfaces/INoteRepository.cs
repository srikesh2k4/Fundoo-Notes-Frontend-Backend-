using DataBaseLayer.Entities;

namespace DataBaseLayer.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetByIdAsync(int noteId);
        Task<IEnumerable<Note>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Note>> GetByIdsAsync(IEnumerable<int> noteIds);
        Task<IEnumerable<Note>> SearchAsync(string query, int userId);
        Task<IEnumerable<Note>> GetTrashedByUserIdAsync(int userId);
        Task AddAsync(Note note);
        Task DeleteAsync(Note note);
        Task DeleteAllTrashedAsync(int userId);
        Task SaveAsync();

        // ✅ FIX: Remove NoteResponseDto, return Note instead
        Task<Note> AddLabelToNoteAsync(int noteId, int labelId, int userId);
        Task<bool> RemoveLabelFromNoteAsync(int noteId, int labelId, int userId);
    }
}
