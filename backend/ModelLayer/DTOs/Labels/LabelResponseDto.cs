namespace ModelLayer.DTOs.Labels
{
    public class LabelResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
