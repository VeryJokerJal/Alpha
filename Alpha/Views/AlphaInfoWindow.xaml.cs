using System.ComponentModel;
using System.Net.Http;
using Alpha.Models;
using Alpha.ViewModels;
using UIShell.Controls;

namespace Alpha.Views
{
    /// <summary>
    /// AlphaInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AlphaInfoWindow : MetroWindow
    {
        public AlphaInfoWindow()
        {
            InitializeComponent();
        }

        public AlphaInfoWindow(AlphaResponse? response, HttpClient? client) : this()
        {
            if (DataContext is AlphaInfoViewModel model)
            {
                model.Client = client;
                model.AlphaResponse = response;
                model.CanSubmit = response?.Is?.Checks?.Any(delegate (AlphaCheck check)
                {
                    string? result = check.Result;
                    return result != null && (result.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                }) == false;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (DataContext is AlphaInfoViewModel model)
            {
                model.CancelSubmit = true;
            }
        }
    }
}
