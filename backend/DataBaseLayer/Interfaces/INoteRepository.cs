using DataBaseLayer.Entities;

namespace DataBaseLayer.Interfaces
{
    public interface INoteRepository
    {
        Task<Note?> GetByIdAsync(int id);
        Task<IEnumerable<Note>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Note>> GetArchivedByUserIdAsync(int userId);
        Task<IEnumerable<Note>> GetDeletedByUserIdAsync(int userId);
        Task<IEnumerable<Note>> GetTrashedByUserIdAsync(int userId);
        Task<IEnumerable<Note>> GetByIdsAsync(IEnumerable<int> ids);
        Task<IEnumerable<Note>> SearchAsync(string query, int userId);
        Task<IEnumerable<Note>> GetByLabelIdAsync(int labelId, int userId);
        Task<IEnumerable<Note>> GetCollaboratedNotesAsync(int userId);
        Task AddAsync(Note note);
        Task DeleteAsync(Note note);
        Task DeleteAllTrashedAsync(int userId);
        Task SaveAsync();
    }
}