using Alpha.Models;
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

        public AlphaInfoWindow(AlphaResponse dataContext) : this()
        {
            DataContext = dataContext;
        }
    }
}
