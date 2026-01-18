namespace ModelLayer.Domain
{
    public class CollaboratorModel
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string? UserName { get; set; }
        public string Permission { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}