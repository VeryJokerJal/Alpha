using System;
using System.Windows;

namespace UIShell.Themes
{
    public abstract class Theme
    {
        public virtual string? Name { get; set; }
        public virtual bool IsDark { get; set; }
        public virtual bool IsLight { get; set; }

        public virtual ResourceDictionary? Current { get; set; }

        public Theme()
        {

        }
        public Theme(string name, bool isDark, bool isLight, ResourceDictionary resourceDictionary)
        {
            Name = name;
            IsDark = isDark;
            IsLight = isLight;
            Current = resourceDictionary;
        }

        public abstract Uri GetResourceUri();
    }
}