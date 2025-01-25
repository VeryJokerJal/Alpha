namespace Alpha.Models
{
    public class FieldData
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int FieldCount { get; set; }
        public List<FieldItem>? Fields { get; set; }
    }
}