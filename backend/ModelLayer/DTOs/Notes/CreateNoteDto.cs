namespace ModelLayer.DTOs.Notes
{
    public class CreateNoteDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Color { get; set; }
        public List<int>? LabelIds { get; set; } // <-- Add this
    }
}