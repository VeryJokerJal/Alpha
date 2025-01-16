namespace Alpha.Models
{
    public class SearchScope
    {
        public string? Region { get; set; }
        public string? Delay { get; set; }
        public string? Universe { get; set; }
        public string? InstrumentType { get; set; }

        public SearchScope()
        {
            Region = null;
            Delay = null;
            Universe = null;
            InstrumentType = null;
        }

        public SearchScope(string? region, string? delay, string? universe, string? instrumentType)
        {
            Region = region;
            Delay = delay;
            Universe = universe;
            InstrumentType = instrumentType;
        }
    }
}
