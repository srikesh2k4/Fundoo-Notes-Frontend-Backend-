namespace DataBaseLayer.Entities
{
    public class Label
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<NoteLabel> NoteLabels { get; set; } = new HashSet<NoteLabel>();
    }
}