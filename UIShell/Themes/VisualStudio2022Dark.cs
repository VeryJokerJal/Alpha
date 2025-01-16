using System;

namespace UIShell.Themes
{
    public class VisualStudio2022Dark : Theme
    {
        public VisualStudio2022Dark()
        {
        }

        public override Uri GetResourceUri()
        {
            return new Uri("/UIShell;component/Themes/VisualStudio2022/DarkTheme.xaml", UriKind.Relative);
        }
    }
}