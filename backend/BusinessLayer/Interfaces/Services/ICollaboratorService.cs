using ModelLayer.DTOs.Collaborators;

namespace BusinessLayer.Interfaces.Services
{
    public interface ICollaboratorService
    {
        Task<IEnumerable<CollaboratorResponseDto>> GetByNoteIdAsync(int noteId, int userId);
        Task<CollaboratorResponseDto> AddAsync(AddCollaboratorDto dto, int ownerUserId);
        Task<CollaboratorResponseDto> UpdatePermissionAsync(int collaboratorId, UpdatePermissionDto dto, int ownerUserId);
        Task RemoveAsync(int collaboratorId, int ownerUserId);
    }
}