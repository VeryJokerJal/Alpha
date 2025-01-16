using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace UIShell.Controls
{
    public class UIButton : ButtonBase
    {
        public SolidColorBrush HoverColor
        {
            get => (SolidColorBrush)GetValue(HoverColorProperty);
            set => SetValue(HoverColorProperty, value);
        }

        public static readonly DependencyProperty HoverColorProperty =
            DependencyProperty.Register(nameof(HoverColor), typeof(SolidColorBrush), typeof(UIButton), new PropertyMetadata(Brushes.Black));

        public SolidColorBrush DefaultColor
        {
            get => (SolidColorBrush)GetValue(DefaultColorProperty);
            set => SetValue(DefaultColorProperty, value);
        }

        public static readonly DependencyProperty DefaultColorProperty =
            DependencyProperty.Register(nameof(DefaultColor), typeof(SolidColorBrush), typeof(UIButton), new PropertyMetadata(0));


    }
}
