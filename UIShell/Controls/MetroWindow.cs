using Microsoft.Xaml.Behaviors;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace UIShell.Controls
{
    public class MetroWindow : Window
    {
        public static bool IsWin11_Or_Latest => Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;

        public static readonly DependencyProperty ActiveGlowBrushProperty = DependencyProperty.Register("ActiveGlowBrush", typeof(SolidColorBrush), typeof(MetroWindow), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty InactiveGlowBrushProperty = DependencyProperty.Register("InactiveGlowBrush", typeof(SolidColorBrush), typeof(MetroWindow), new PropertyMetadata(Brushes.Transparent));

        /// <summary>
        /// Left side of title bar
        /// </summary>
        public static readonly DependencyProperty LeftWindowCommandsProperty = DependencyProperty.Register(nameof(LeftWindowCommands), typeof(FrameworkElement), typeof(MetroWindow));

        /// <summary>
        /// Right side of title bar
        /// </summary>
        public static readonly DependencyProperty RightWindowCommandsProperty = DependencyProperty.Register(nameof(RightWindowCommands), typeof(FrameworkElement), typeof(MetroWindow));

        /// <summary>
        /// Sets whether Icon is displayed
        /// </summary>
        public static readonly DependencyProperty IsShowIconProperty = DependencyProperty.Register(nameof(IsShowIcon), typeof(bool), typeof(MetroWindow), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether Title is displayed
        /// </summary>
        public static readonly DependencyProperty IsShowTitleProperty = DependencyProperty.Register(nameof(IsShowTitle), typeof(bool), typeof(MetroWindow), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether TitleBar is displayed
        /// </summary>
        public static readonly DependencyProperty IsShowTitleBarProperty = DependencyProperty.Register(nameof(IsShowTitleBar), typeof(bool), typeof(MetroWindow), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether MinimizeButton is displayed
        /// </summary>
        public static readonly DependencyProperty IsShowMinimizeButtonProperty = DependencyProperty.Register(nameof(IsShowMinimizeButton), typeof(bool), typeof(MetroWindow), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether MaximizeButton is displayed
        /// </summary>
        public static readonly DependencyProperty IsShowMaximizeButtonProperty = DependencyProperty.Register(nameof(IsShowMaximizeButton), typeof(bool), typeof(MetroWindow), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether CloseButton is displayed
        /// </summary>
        public static readonly DependencyProperty IsShowCloseButtonProperty = DependencyProperty.Register(nameof(IsShowCloseButton), typeof(bool), typeof(MetroWindow), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether CloseButton is displayed
        /// </summary>
        public static readonly DependencyProperty OnMaximizedPaddingProperty = DependencyProperty.Register(nameof(OnMaximizedPadding), typeof(Thickness), typeof(MetroWindow));


        public SolidColorBrush ActiveGlowBrush
        {
            get => (SolidColorBrush)GetValue(ActiveGlowBrushProperty);
            set => SetValue(ActiveGlowBrushProperty, value);
        }

        public SolidColorBrush InactiveGlowBrush
        {
            get => (SolidColorBrush)GetValue(InactiveGlowBrushProperty);
            set => SetValue(InactiveGlowBrushProperty, value);
        }

        public FrameworkElement LeftWindowCommands
        {
            get => (FrameworkElement)GetValue(LeftWindowCommandsProperty);
            set => SetValue(LeftWindowCommandsProperty, value);
        }

        public FrameworkElement RightWindowCommands
        {
            get => (FrameworkElement)GetValue(RightWindowCommandsProperty);
            set => SetValue(RightWindowCommandsProperty, value);
        }

        public bool IsShowIcon
        {
            get => (bool)GetValue(IsShowIconProperty);
            set => SetValue(IsShowIconProperty, value);
        }

        public bool IsShowTitle
        {
            get => (bool)GetValue(IsShowTitleProperty);
            set => SetValue(IsShowTitleProperty, value);
        }

        public bool IsShowTitleBar
        {
            get => (bool)GetValue(IsShowTitleBarProperty);
            set => SetValue(IsShowTitleBarProperty, value);
        }

        public bool IsShowMinimizeButton
        {
            get => (bool)GetValue(IsShowMinimizeButtonProperty);
            set => SetValue(IsShowMinimizeButtonProperty, value);
        }

        public bool IsShowMaximizeButton
        {
            get => (bool)GetValue(IsShowMaximizeButtonProperty);
            set => SetValue(IsShowMaximizeButtonProperty, value);
        }

        public bool IsShowCloseButton
        {
            get => (bool)GetValue(IsShowCloseButtonProperty);
            set => SetValue(IsShowCloseButtonProperty, value);
        }

        public Thickness OnMaximizedPadding
        {
            get => (Thickness)GetValue(OnMaximizedPaddingProperty);
            set => SetValue(OnMaximizedPaddingProperty, value);
        }

        static MetroWindow()
        {
            OnMaximizedPaddingProperty.OverrideMetadata(typeof(MetroWindow), new FrameworkPropertyMetadata(IsWin11_Or_Latest ? new Thickness(0) : new Thickness(8)));
            StyleProperty.OverrideMetadata(typeof(MetroWindow), new FrameworkPropertyMetadata(Application.Current.TryFindResource("MetroWindowBaseStyle")));
        }

        public MetroWindow()
        {
        }

        private void InitializeGlowWindowBehaviorEx()
        {
            ControlzEx.Behaviors.GlowWindowBehavior behavior = new();
            _ = BindingOperations.SetBinding(behavior, ControlzEx.Behaviors.GlowWindowBehavior.GlowColorProperty, new Binding { Path = new PropertyPath("ActiveGlowBrush.Color"), Source = this });
            _ = BindingOperations.SetBinding(behavior, ControlzEx.Behaviors.GlowWindowBehavior.NonActiveGlowColorProperty, new Binding { Path = new PropertyPath("InactiveGlowBrush.Color"), Source = this });

            Interaction.GetBehaviors(this).Add(behavior);
        }

        private void InitializeWindowChromeEx()
        {
            Interaction.GetBehaviors(this).Add(new ControlzEx.Behaviors.WindowChromeBehavior());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            InitializeGlowWindowBehaviorEx();
            InitializeWindowChromeEx();
        }

        [DllImport("user32.dll")]
        public static extern int MessageBeep(uint uType);

        /// <summary>
        /// 错误提示音
        /// </summary>
        public const uint MB_ICONERROR = 0x00000010;
        /// <summary>
        /// 问题提示音
        /// </summary>
        public const uint MB_ICONQUESTION = 0x00000020;
        /// <summary>
        /// 警告提示音
        /// </summary>
        public const uint MB_ICONWARNING = 0x00000030;
        /// <summary>
        /// 信息提示音
        /// </summary>
        public const uint MB_ICONINFORMATION = 0x00000040;
        /// <summary>
        /// 默认提示音
        /// </summary>
        public const uint MB_OK = 0x00000000;
    }
}
