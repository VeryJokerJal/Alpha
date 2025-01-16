using Alpha.ViewModels;
using ReactiveUI.Fody.Helpers;

namespace Alpha.Models
{
    public class SimulationSettingsModel : ViewModelBase
    {
        [Reactive]
        public string? ProgrammingLanguage { get; set; }
        [Reactive]
        public string? AssetClass { get; set; }
        [Reactive]
        public string? Region { get; set; }
        [Reactive]
        public string? Liquidity { get; set; }
        [Reactive]
        public string? DecisionDelay { get; set; }
        [Reactive]
        public string? AlphaWeight { get; set; }
        [Reactive]
        public string? DataCleaning { get; set; }
        [Reactive]
        public string? WarningThrowing { get; set; }
        [Reactive]
        public string? MissingValueHandling { get; set; }
        [Reactive]
        public string? LinearDecayWeightedAverage { get; set; } = "4";
        [Reactive]
        public string? TruncationWeight { get; set; } = "0.08";
        [Reactive]
        public string? TimeEvaluationYear { get; set; } = "1";
        [Reactive]
        public string? TimeEvaluationMonth { get; set; } = "0";
    }
}
