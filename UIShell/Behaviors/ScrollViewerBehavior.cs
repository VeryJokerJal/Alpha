using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace UIShell.Behaviors
{
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));
        public static void SetHorizontalOffset(FrameworkElement target, double value)
        {
            target.SetValue(HorizontalOffsetProperty, value);
        }

        public static double GetHorizontalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(HorizontalOffsetProperty);
        }

        private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            (target as ScrollViewer)?.ScrollToHorizontalOffset((double)e.NewValue);
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));
        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            (target as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
        }
    }

    public static class ScrollViewerBehaviors
    {
        public static readonly DependencyProperty EnableScrollAnimationProperty =
            DependencyProperty.RegisterAttached(
                "EnableScrollAnimation",
                typeof(bool),
                typeof(ScrollViewerBehaviors),
                new PropertyMetadata(false, OnEnableScrollAnimationChanged));

        public static bool GetEnableScrollAnimation(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableScrollAnimationProperty);
        }

        public static void SetEnableScrollAnimation(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableScrollAnimationProperty, value);
        }

        public static readonly DependencyProperty LastVerticalLocationProperty =
            DependencyProperty.RegisterAttached(
                "LastVerticalLocation",
                typeof(double),
                typeof(ScrollViewerBehaviors),
                new PropertyMetadata(0.0));

        public static double GetLastVerticalLocation(DependencyObject obj)
        {
            return (double)obj.GetValue(LastVerticalLocationProperty);
        }

        public static void SetLastVerticalLocation(DependencyObject obj, double value)
        {
            obj.SetValue(LastVerticalLocationProperty, value);
        }

        public static readonly DependencyProperty LastHorizontalLocationProperty =
            DependencyProperty.RegisterAttached(
                "LastHorizontalLocation",
                typeof(double),
                typeof(ScrollViewerBehaviors),
                new PropertyMetadata(0.0));

        public static double GetLastHorizontalLocation(DependencyObject obj)
        {
            return (double)obj.GetValue(LastHorizontalLocationProperty);
        }

        public static void SetLastHorizontalLocation(DependencyObject obj, double value)
        {
            obj.SetValue(LastHorizontalLocationProperty, value);
        }

        private static void OnEnableScrollAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_MouseWheel;
                    scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_MouseWheel;
                    scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                }
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                SetLastVerticalLocation(scrollViewer, e.VerticalOffset);
                SetLastHorizontalLocation(scrollViewer, e.HorizontalOffset);
            }
        }

        private static void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer && !scrollViewer.CanContentScroll)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)  // 按住Shift键进行横向滚动
                {
                    double lastHorizontalLocation = GetLastHorizontalLocation(scrollViewer);
                    double wheelChange = e.Delta / 2;
                    double newOffset = lastHorizontalLocation - wheelChange;

                    newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableWidth));

                    // 使用自定义动画进行滚动
                    AnimateHorizontalScroll(scrollViewer, newOffset);
                    SetLastHorizontalLocation(scrollViewer, newOffset);
                }
                else  // 纵向滚动
                {
                    double lastVerticalLocation = GetLastVerticalLocation(scrollViewer);
                    double wheelChange = e.Delta / 2;
                    double newOffset = lastVerticalLocation - wheelChange;

                    newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));

                    AnimateVerticalScroll(scrollViewer, newOffset);
                    SetLastVerticalLocation(scrollViewer, newOffset);
                }
                e.Handled = true;
            }
        }

        private static void AnimateVerticalScroll(ScrollViewer scrollViewer, double toValue)
        {
            DoubleAnimation animation = new()
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                From = scrollViewer.VerticalOffset,
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            SetLastVerticalLocation(scrollViewer, toValue);
            scrollViewer.BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
        }

        private static void AnimateHorizontalScroll(ScrollViewer scrollViewer, double toValue)
        {
            DoubleAnimation animation = new()
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                From = scrollViewer.HorizontalOffset,
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            SetLastHorizontalLocation(scrollViewer, toValue);
            scrollViewer.BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, animation);
        }
    }

    public class AScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty EnableScrollAnimationProperty =
            DependencyProperty.Register(
                "EnableScrollAnimation",
                typeof(bool),
                typeof(AScrollViewer),
                new PropertyMetadata(false));

        public bool EnableScrollAnimation
        {
            get => (bool)GetValue(EnableScrollAnimationProperty);
            set => SetValue(EnableScrollAnimationProperty, value);
        }

        public static readonly DependencyProperty LastVerticalLocationProperty =
            DependencyProperty.Register(
                "LastVerticalLocation",
                typeof(double),
                typeof(AScrollViewer),
                new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            (target as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
        }

        public double LastVerticalLocation
        {
            get => (double)GetValue(LastVerticalLocationProperty);
            set => SetValue(LastVerticalLocationProperty, value);
        }

        public static readonly DependencyProperty LastHorizontalLocationProperty =
            DependencyProperty.Register(
                "LastHorizontalLocation",
                typeof(double),
                typeof(AScrollViewer),
                new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

        private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            (target as ScrollViewer)?.ScrollToHorizontalOffset((double)e.NewValue);
        }

        public double LastHorizontalLocation
        {
            get => (double)GetValue(LastHorizontalLocationProperty);
            set => SetValue(LastHorizontalLocationProperty, value);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!EnableScrollAnimation)
            {
                base.OnMouseWheel(e);
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Shift)  // 按住Shift键进行横向滚动
            {
                double wheelChange = e.Delta / 2;
                double newOffset = LastHorizontalLocation - wheelChange;

                newOffset = Math.Max(0, Math.Min(newOffset, ScrollableWidth));

                // 使用自定义动画进行滚动
                AnimateHorizontalScroll(newOffset);
                LastHorizontalLocation = newOffset;
            }
            else  // 纵向滚动
            {
                double wheelChange = e.Delta / 2;
                double newOffset = LastVerticalLocation - wheelChange;

                newOffset = Math.Max(0, Math.Min(newOffset, ScrollableHeight));

                AnimateVerticalScroll(newOffset);
                LastVerticalLocation = newOffset;
            }

            e.Handled = true;
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            LastVerticalLocation = e.VerticalOffset;
            LastHorizontalLocation = e.HorizontalOffset;
        }

        private void AnimateVerticalScroll(double toValue)
        {
            DoubleAnimation animation = new()
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                From = VerticalOffset,
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            LastVerticalLocation = toValue;
            BeginAnimation(LastVerticalLocationProperty, animation);
        }

        private void AnimateHorizontalScroll(double toValue)
        {
            BeginAnimation(LastHorizontalLocationProperty, null);

            DoubleAnimation animation = new()
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                From = HorizontalOffset,
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            LastHorizontalLocation = toValue;
            BeginAnimation(LastHorizontalLocationProperty, animation);
        }
    }
}
