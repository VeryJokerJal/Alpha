using System.IO;
using Alpha.Models;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Alpha.Properties
{
    public class UserSettings : ReactiveObject
    {
        [JsonProperty][Reactive] public string? Email { get; set; }
        [JsonProperty][Reactive] public string? Password { get; set; }
        [JsonProperty][Reactive] public int BackTestModeIndex { get; set; }
        [JsonProperty][Reactive] public int ProgrammingLanguageIndex { get; set; }
        [JsonProperty][Reactive] public int AssetClassIndex { get; set; }
        [JsonProperty][Reactive] public int RegionIndex { get; set; }
        [JsonProperty][Reactive] public int LiquidityIndex { get; set; }
        [JsonProperty][Reactive] public int DecisionDelayIndex { get; set; }
        [JsonProperty][Reactive] public int AlphaWeightIndex { get; set; }
        [JsonProperty][Reactive] public int DataCleaningIndex { get; set; }
        [JsonProperty][Reactive] public int WarningThrowingIndex { get; set; }
        [JsonProperty][Reactive] public int MissingValueHandlingIndex { get; set; }
        [JsonProperty][Reactive] public bool AutoSubmit { get; set; }
        [JsonProperty][Reactive] public bool IgnoreWarnings { get; set; }
        [JsonProperty][Reactive] public bool IgnoreErrors { get; set; }
        [JsonProperty][Reactive] public bool IgnoreStop { get; set; }
        [JsonProperty][Reactive] public bool IgnorePasses { get; set; }
        [JsonProperty][Reactive] public bool IgnoreCompleted { get; set; }
        [JsonProperty][Reactive] public bool IgnoreFailures { get; set; }
        [JsonProperty][Reactive] public bool IautoSubmit { get; set; }
        [JsonProperty][Reactive] public SimulationSettingsModel SimulationSettings { get; set; }

        private readonly SemaphoreSlim semaphoreSlim;

        public UserSettings()
        {
            semaphoreSlim = new SemaphoreSlim(1);
            SimulationSettings = new SimulationSettingsModel();
            PropertyChanged += (_, _) => SaveSettings();
            SimulationSettings.PropertyChanged += (_, _) => SaveSettings();
        }

        private async void SaveSettings()
        {
            await semaphoreSlim.WaitAsync();
            string settingsJson = JsonConvert.SerializeObject(this);
            await File.WriteAllTextAsync("settings.json", settingsJson);
            _ = semaphoreSlim.Release();
        }

        public static UserSettings LoadSettings()
        {
            if (File.Exists("settings.json"))
            {
                string settingsJson = File.ReadAllText("settings.json");
                return JsonConvert.DeserializeObject<UserSettings>(settingsJson) ?? new UserSettings();
            }
            return new UserSettings();
        }
    }
}
