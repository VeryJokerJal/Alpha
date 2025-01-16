using System;

namespace UIShell.Themes
{
    public class VisualStudio2019Blue : Theme
    {
        public VisualStudio2019Blue()
        {
        }

        public override Uri GetResourceUri()
        {
            return new Uri("/UIShell;component/Themes/VisualStudio2019/BlueTheme.xaml", UriKind.Relative);
        }
    }
}