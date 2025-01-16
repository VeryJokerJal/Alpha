using System;

namespace UIShell.Themes
{
    public class VisualStudio2019Dark : Theme
    {
        public VisualStudio2019Dark()
        {
        }

        public override Uri GetResourceUri()
        {
            return new Uri("/UIShell;component/Themes/VisualStudio2019/DarkTheme.xaml", UriKind.Relative);
        }
    }
}