namespace ModelLayer.DTOs.Notes
{
    public class NoteResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; } // ✅ ADD: UserId for consistency
        public string? Title { get; set; } // ✅ FIX: Should be nullable
        public string? Content { get; set; } // ✅ FIX: Should be nullable
        public string Color { get; set; } = "#FFFFFF"; // ✅ FIX: Default value
        public bool IsPinned { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // ✅ ADD: DeletedAt for trash functionality
        public List<LabelDto> Labels { get; set; } = new(); // ✅ FIX: Initialize empty list
    }

    public class LabelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // ✅ FIX: Use string.Empty instead of null!
    }
}
