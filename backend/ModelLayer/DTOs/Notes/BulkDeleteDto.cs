namespace ModelLayer.DTOs.Notes
{
    public class BulkDeleteDto
    {
        public IEnumerable<int> NoteIds { get; set; } = new List<int>();
    }
}
