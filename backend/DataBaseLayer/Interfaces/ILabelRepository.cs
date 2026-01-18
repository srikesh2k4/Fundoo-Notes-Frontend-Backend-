using DataBaseLayer.Entities;

namespace DataBaseLayer.Interfaces
{
    public interface ILabelRepository
    {
        Task<Label?> GetByIdAsync(int id);
        Task<IEnumerable<Label>> GetByUserAsync(int userId);
        Task<bool> ExistsForUserAsync(string name, int userId, int? excludeLabelId = null);
        Task AddAsync(Label label);
        Task DeleteAsync(Label label);
        Task SaveAsync();
    }
}