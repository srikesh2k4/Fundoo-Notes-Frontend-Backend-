using ModelLayer.Enums;

namespace ModelLayer.DTOs.Collaborators
{
    public class CollaboratorResponseDto
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? UserName { get; set; }
        public PermissionLevel Permission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}