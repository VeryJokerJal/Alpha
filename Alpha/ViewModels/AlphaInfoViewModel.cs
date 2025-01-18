using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using Alpha.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Alpha.ViewModels
{
    public class AlphaInfoViewModel : ViewModelBase
    {
        public HttpClient? Client { get; set; }
        public ICommand SubmitCommand { get; }
        [Reactive]
        public AlphaResponse? AlphaResponse { get; set; }
        [Reactive]
        public bool CancelSubmit { get; set; }
        [Reactive]
        public bool CanSubmit { get; set; }

        public AlphaInfoViewModel()
        {
            SubmitCommand = ReactiveCommand.CreateFromTask(Submit, outputScheduler: this);
            CancelSubmit = false;
        }

        private async Task Submit()
        {
            if (AlphaResponse?.Id == null)
            {
                _ = MessageBox.Show("Alpha 不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (await SubmitAlpha(AlphaResponse.Id))
            {
                _ = MessageBox.Show("提交成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (CancelSubmit)
                {
                    return;
                }
                _ = MessageBox.Show("提交失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> SubmitAlpha(string? alphaId)
        {
            if (Client == null || string.IsNullOrEmpty(alphaId))
            {
                return false;
            }

            string url = $"https://api.worldquantbrain.com/alphas/{alphaId}/submit";
            HttpResponseMessage response = await Client.PostAsync(url, null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            string? getUrl = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(getUrl))
            {
                return false;
            }

            while (!CancelSubmit)
            {
                HttpResponseMessage getResponse = await Client.GetAsync(getUrl);

                if (getResponse.Headers.TryGetValues("Retry-After", out IEnumerable<string>? retryAfterValues))
                {
                    string retryAfterStr = retryAfterValues.FirstOrDefault() ?? "0";
                    double retryAfterSec = double.Parse(retryAfterStr);
                    await Task.Delay(TimeSpan.FromSeconds(retryAfterSec * 5));
                }
                else
                {
                    return getResponse.StatusCode != HttpStatusCode.Forbidden;
                }
            }

            return false;
        }
    }
}
