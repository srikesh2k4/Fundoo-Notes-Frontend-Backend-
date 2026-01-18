namespace ModelLayer.DTOs.Notes
{
    public class NoteResponseDto
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
        public List<LabelDto>? Labels { get; set; }
    }

    public class LabelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}