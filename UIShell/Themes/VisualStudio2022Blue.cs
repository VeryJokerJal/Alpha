using System;

namespace UIShell.Themes
{
    public class VisualStudio2022Blue : Theme
    {
        public VisualStudio2022Blue()
        {
        }

        public override Uri GetResourceUri()
        {
            return new Uri("/UIShell;component/Themes/VisualStudio2022/BlueTheme.xaml", UriKind.Relative);
        }
    }
}