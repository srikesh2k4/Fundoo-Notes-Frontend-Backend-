using DataBaseLayer.Entities;

namespace DataBaseLayer.Interfaces
{
    public interface ICollaboratorRepository
    {
        Task<Collaborator?> GetByIdAsync(int id);
        Task<Collaborator?> GetByIdWithUserAsync(int id);
        Task<Collaborator?> GetByNoteAndUserAsync(int noteId, int userId);
        Task<IEnumerable<Collaborator>> GetByNoteIdAsync(int noteId);
        Task<bool> ExistsAsync(int noteId, int collaboratorUserId);
        Task AddAsync(Collaborator collaborator);
        Task DeleteAsync(Collaborator collaborator);
        Task SaveAsync();
    }
}