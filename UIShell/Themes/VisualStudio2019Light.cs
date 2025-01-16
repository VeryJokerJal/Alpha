using System;

namespace UIShell.Themes
{
    public class VisualStudio2019Light : Theme
    {
        public VisualStudio2019Light()
        {
        }

        public override Uri GetResourceUri()
        {
            return new Uri("/UIShell;component/Themes/VisualStudio2019/LightTheme.xaml", UriKind.Relative);
        }
    }
}