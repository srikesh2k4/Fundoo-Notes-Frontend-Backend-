using ModelLayer.DTOs.Labels;

namespace BusinessLayer.Interfaces.Services
{
    public interface ILabelService
    {
        Task<IEnumerable<LabelResponseDto>> GetByUserAsync(int userId);
        Task<LabelResponseDto?> GetByIdAsync(int labelId, int userId);
        Task<LabelResponseDto> CreateAsync(CreateLabelDto dto, int userId);
        Task<LabelResponseDto> UpdateAsync(int labelId, UpdateLabelDto dto, int userId);
        Task DeleteAsync(int labelId, int userId);
    }
}