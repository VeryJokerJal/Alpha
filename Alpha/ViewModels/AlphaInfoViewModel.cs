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
        [Reactive] public AlphaResponse? AlphaResponse { get; set; }
        [Reactive] public bool CancelSubmit { get; set; }
        [Reactive] public bool CanSubmit { get; set; }

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

            SubmitResult result = await SubmitAlpha(AlphaResponse.Id);
            if (result.Success)
            {
                _ = MessageBox.Show("提交成功。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (CancelSubmit)
                {
                    return;
                }
                _ = MessageBox.Show(result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<SubmitResult> SubmitAlpha(string? alphaId)
        {
            SubmitResult submitResult = new();
            if (Client == null || string.IsNullOrEmpty(alphaId))
            {
                submitResult.Message = "Alpha 不存在。";
                return submitResult;
            }

            string url = $"https://api.worldquantbrain.com/alphas/{alphaId}/submit";
            HttpResponseMessage response = await Client.PostAsync(url, null);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                submitResult.Success = true;
                submitResult.Message = "提交成功。";
                return submitResult;
            }
            else if (response.StatusCode != HttpStatusCode.Created)
            {
                submitResult.Message = "请求失败。(" + (int)response.StatusCode + ")";
                return submitResult;
            }

            while (!CancelSubmit)
            {
                HttpResponseMessage getResponse = await Client.GetAsync(url);

                if (getResponse.Headers.TryGetValues("Retry-After", out IEnumerable<string>? retryAfterValues))
                {
                    string retryAfterStr = retryAfterValues.FirstOrDefault() ?? "0";
                    double retryAfterSec = double.Parse(retryAfterStr);
                    await Task.Delay(TimeSpan.FromSeconds(retryAfterSec * 5));
                }
                else
                {
                    if (getResponse.StatusCode == HttpStatusCode.OK)
                    {
                        submitResult.Success = true;
                        submitResult.Message = "提交成功。";
                    }
                    else
                    {
                        submitResult.Message = getResponse.StatusCode == HttpStatusCode.SeeOther ? "请稍后再试。(303)" : "请求失败。(" + (int)getResponse.StatusCode + ")";
                    }
                    return submitResult;
                }
            }

            return submitResult;
        }
    }
}
