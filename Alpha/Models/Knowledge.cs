namespace Alpha.Models
{
    public class Knowledge
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int FieldCount { get; set; }
        public List<Field>? Fields { get; set; }
    }
}