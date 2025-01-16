namespace Alpha.Helpers
{
    public static class TimeHelper
    {
        public static string GetTimeSpan(DateTime start, DateTime end)
        {
            TimeSpan timeSpan = end - start;
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        public static string FormatTime(this TimeSpan time)
        {
            if (time.TotalSeconds < 60)
            {
                return $"{time.Seconds}秒";
            }
            else if (time.TotalSeconds < 3600)
            {
                int minutes = time.Minutes;
                int seconds = time.Seconds;
                return $"{minutes}分 {seconds}秒";
            }
            else if (time.TotalSeconds < 86400)
            {
                int hours = time.Hours;
                int minutes = time.Minutes;
                return $"{hours}小时 {minutes}分";
            }
            else
            {
                int days = time.Days;
                int hours = time.Hours;
                return $"{days}天 {hours}小时";
            }
        }
    }
}
