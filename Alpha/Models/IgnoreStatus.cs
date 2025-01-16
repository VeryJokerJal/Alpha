namespace Alpha.Models
{
    [Flags]
    public enum IgnoreStatus
    {
        None = 0,
        Warning = 1,
        Error = 2,
        Stop = 4,
        Pass = 8,
        Complete = 16,
        Failure = 32
    }
}
