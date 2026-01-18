namespace DataBaseLayer.Entities
{
    public class NoteLabel
    {
        public int NoteId { get; set; }
        public int LabelId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public Note Note { get; set; } = null!;
        public Label Label { get; set; } = null!;
    }
}