namespace Alpha.Properties
{
    public static class Settings
    {
        public static UserSettings User { get; }

        static Settings()
        {
            User = UserSettings.LoadSettings();
        }
    }
}
