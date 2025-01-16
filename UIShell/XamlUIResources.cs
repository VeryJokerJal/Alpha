using System;
using System.Windows;
using UIShell.Themes;

namespace UIShell
{
    public class XamlUIResources : ResourceDictionary
    {
        private static XamlUIResources? instance;
        public static XamlUIResources Instance => instance ??= new XamlUIResources();

        public Theme Theme
        {
            get => theme;
            set => CoerceSetTheme(theme = value);
        }

        public XamlUIResources()
        {
            theme = new VisualStudio2022Dark();
            CoerceInitialize();

            instance ??= this;
        }

        private Theme theme;

        private void CoerceInitialize()
        {
            MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/UIShell;component/Styles/Controls.xaml", UriKind.Relative)
            });
            MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = theme.GetResourceUri()
            });
        }
        private void CoerceSetTheme(Theme theme)
        {
            MergedDictionaries[1] = new ResourceDictionary()
            {
                Source = theme.GetResourceUri()
            };
        }
    }
}
