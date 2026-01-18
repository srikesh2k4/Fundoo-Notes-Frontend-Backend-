namespace DataBaseLayer.Entities
{
    public class Note
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string Color { get; set; } = "#FFFFFF";
        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int UserId { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<NoteLabel> NoteLabels { get; set; } = new HashSet<NoteLabel>();
        public ICollection<Collaborator> Collaborators { get; set; } = new HashSet<Collaborator>();
    }
}