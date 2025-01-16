﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UIShell.Converters
{
    [ValueConversion(typeof(TreeViewItem), typeof(Thickness))]
    public class TreeViewMarginConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is TreeViewItem item ? new Thickness(Length * item.GetDepth(), 0, 0, 0) : (object)new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            // Use non linq version for better performance (maybe).
            return item.CountAncestors<TreeView>(x => x is TreeViewItem);
        }

        /// <summary>
        /// Counts the ancestors until the type T was found.
        /// </summary>
        /// <typeparam name="T">The type until the search should work.</typeparam>
        /// <param name="child">The start child.</param>
        /// <param name="countFor">A filter function which can be null to count all ancestors.</param>
        /// <returns></returns>
        internal static int CountAncestors<T>(this DependencyObject child, Func<DependencyObject, bool> countFor)
        where T : DependencyObject
        {
            int count = 0;
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && parent.GetType() != typeof(T))
            {
                if (countFor is null || countFor(parent))
                {
                    count++;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return count;
        }
    }
}
