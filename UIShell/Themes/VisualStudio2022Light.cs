using System;

namespace UIShell.Themes
{
    public class VisualStudio2022Light : Theme
    {
        public VisualStudio2022Light()
        {
        }

        public override Uri GetResourceUri()
        {
            return new Uri("/UIShell;component/Themes/VisualStudio2022/LightTheme.xaml", UriKind.Relative);
        }
    }
}