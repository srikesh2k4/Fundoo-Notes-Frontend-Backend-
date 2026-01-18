using DataBaseLayer.Enums;

namespace DataBaseLayer.Entities
{
    public class Collaborator
    {
        public int Id { get; set; }

        // Foreign Keys
        public int NoteId { get; set; }
        public int CollaboratorId { get; set; }

        // Permission
        public PermissionLevel Permission { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public Note Note { get; set; } = null!;
        public User CollaboratorUser { get; set; } = null!;
    }
}