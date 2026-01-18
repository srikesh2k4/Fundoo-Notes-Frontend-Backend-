namespace ModelLayer.Domain
{
    public class NoteModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Color { get; set; } = null!;
        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public List<LabelModel>? Labels { get; set; }
        public List<CollaboratorModel>? Collaborators { get; set; }
    }
}