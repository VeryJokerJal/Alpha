﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using UIShell.Helpers;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace UIShell.Controls.Attach
{
    public class MenuTopLineAttach
    {
        private const double SystemPopupRightPadding = 6.0d;

        public static readonly DependencyProperty PopupProperty = DependencyProperty.RegisterAttached(
        "Popup", typeof(Popup), typeof(MenuTopLineAttach), new PropertyMetadata(default(Popup), OnPopupChanged));

        public static readonly DependencyProperty TopLineProperty = DependencyProperty.RegisterAttached(
        "TopLine", typeof(FrameworkElement), typeof(MenuTopLineAttach), new PropertyMetadata(default(FrameworkElement)));

        private static void OnPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement topLine = (FrameworkElement)d;

            if (e.NewValue is Popup popup && popup.TemplatedParent is MenuItem menuItem)
            {
                SetTopLine(menuItem, topLine);
                menuItem.Loaded += MenuItem_Loaded;
            }
        }

        private static void MenuItem_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement menuItem = (FrameworkElement)sender;
            menuItem.Unloaded += MenuItem_Unloaded;
            FrameworkElement topLine = GetTopLine(menuItem);
            if (topLine is null)
            {
                return;
            }

            Popup popup = GetPopup(topLine);
            if (popup is not null)
            {
                popup.Opened += Popup_Opened;
            }
        }

        private static void MenuItem_Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement menuItem = (FrameworkElement)sender;
            menuItem.Unloaded -= MenuItem_Unloaded;
            FrameworkElement topLine = GetTopLine(menuItem);
            if (topLine is null)
            {
                return;
            }

            Popup popup = GetPopup(topLine);
            if (popup is not null)
            {
                popup.Opened -= Popup_Opened;
            }
        }

        private static void Popup_Opened(object? sender, EventArgs e)
        {
            if (sender is null)
            {
                return;
            }

            Popup popup = (Popup)sender;
            if (popup.TemplatedParent is MenuItem menuItem)
            {
                FrameworkElement topLine = GetTopLine(menuItem);
                if (topLine is null)
                {
                    return;
                }

                Panel panel = VisualHelper.GetParent<Panel>(topLine);
                if (panel is null)
                {
                    return;
                }

                if (!GetWorkArea(out RECT workAreaRect))
                {
                    return;
                }

                System.Windows.Media.Matrix matrix = PresentationSource.FromVisual(menuItem).CompositionTarget.TransformToDevice;
                Point workAreaRectDpi = new(workAreaRect.right / matrix.M11, workAreaRect.bottom / matrix.M22);

                Point menuItemLeftTop = menuItem.PointToScreen(new Point());
                menuItemLeftTop = new Point(menuItemLeftTop.X / matrix.M11, menuItemLeftTop.Y / matrix.M22);
                Point menuItemRightBottom = new(menuItemLeftTop.X + menuItem.ActualWidth, menuItemLeftTop.Y + menuItem.ActualHeight);

                if (menuItemRightBottom.Y > workAreaRectDpi.Y ||
                menuItemRightBottom.Y + panel.ActualHeight > workAreaRectDpi.Y)
                {
                    // Bottom
                    topLine.Width = 0;
                    topLine.Margin = new Thickness();
                }
                else
                {
                    double leftPadding = menuItem.BorderThickness.Left;
                    double rightPadding = menuItem.BorderThickness.Right;
                    double leftRightPadding = leftPadding + rightPadding;

                    double left, top, width;
                    top = -menuItem.BorderThickness.Top;

                    // High Dpi
                    double remainderDpi = matrix.M11 - 1;

                    popup.HorizontalOffset = -1;
                    if (menuItemLeftTop.X < 0)
                    {
                        // Left
                        left = menuItemLeftTop.X - panel.Margin.Left;
                        width = menuItem.ActualWidth - leftRightPadding;

                        popup.HorizontalOffset = 2.0;
                    }
                    else if (menuItemLeftTop.X + panel.ActualWidth + SystemPopupRightPadding >= workAreaRectDpi.X)
                    {
                        // Right
                        double overflowWidth = menuItemRightBottom.X + SystemPopupRightPadding - workAreaRectDpi.X;
                        if (overflowWidth >= 0)
                        {
                            width = workAreaRectDpi.X - SystemPopupRightPadding - menuItemLeftTop.X - leftPadding;
                            if (width < 0)
                            {
                                width = 0;
                            }
                        }
                        else
                        {
                            width = menuItemRightBottom.X - menuItemLeftTop.X - leftRightPadding;
                        }
                        left = menuItemLeftTop.X - (workAreaRectDpi.X - SystemPopupRightPadding - panel.ActualWidth) + leftPadding;

                        left -= remainderDpi;
                        width += remainderDpi;
                    }
                    else
                    {
                        // Normal
                        left = 0;
                        width = menuItemRightBottom.X - menuItemLeftTop.X - leftRightPadding;

                        if (remainderDpi is > 0 and < 0.5)
                        {
                            width += remainderDpi;
                        }
                    }


                    // Fix High Dpi
                    if (remainderDpi >= 0.5)
                    {
                        popup.HorizontalOffset = -remainderDpi;

                        Point childPoint = popup.Child.PointToScreen(new Point());
                        childPoint = new Point(childPoint.X / matrix.M11, childPoint.Y / matrix.M22);

                        if (childPoint.X > menuItemLeftTop.X)
                        {
                            double xlPadding = childPoint.X - menuItemLeftTop.X;

                            popup.HorizontalOffset -= xlPadding;
                            width -= xlPadding;
                        }
                        else if (childPoint.X + left < menuItemLeftTop.X)
                        {
                            double xrPadding = menuItemLeftTop.X - (childPoint.X + left);
                            left += xrPadding;

                            width -= 1;
                            if (menuItemRightBottom.X > workAreaRectDpi.X - SystemPopupRightPadding)
                            {
                                width -= 1 - Math.Abs(remainderDpi);
                                if (width < 0)
                                {
                                    width = 0;
                                }

                                left += xrPadding;
                            }
                        }
                    }

                    topLine.Width = width;
                    topLine.Margin = new Thickness(left, top, 0, 0);
                }
            }
        }

        public static void SetPopup(DependencyObject element, Popup value)
        {
            element.SetValue(PopupProperty, value);
        }

        public static Popup GetPopup(DependencyObject element)
        {
            return (Popup)element.GetValue(PopupProperty);
        }

        public static void SetTopLine(DependencyObject element, FrameworkElement value)
        {
            element.SetValue(TopLineProperty, value);
        }

        public static FrameworkElement GetTopLine(DependencyObject element)
        {
            return (FrameworkElement)element.GetValue(TopLineProperty);
        }

        private static BOOL GetWorkArea(out RECT rect)
        {
            unsafe
            {
                fixed (void* ptr = &rect)
                {
                    return PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA, 0, ptr, 0);
                }
            }
        }
    }
}
