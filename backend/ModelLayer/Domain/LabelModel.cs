namespace ModelLayer.Domain
{
    public class LabelModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}