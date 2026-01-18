using ModelLayer.Enums;

namespace ModelLayer.DTOs.Collaborators
{
    public class AddCollaboratorDto
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public PermissionLevel Permission { get; set; }
    }
}