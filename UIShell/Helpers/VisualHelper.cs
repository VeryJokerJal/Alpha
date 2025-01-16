using System.Windows;
using System.Windows.Media;

namespace UIShell.Helpers
{
    internal static class VisualHelper
    {
        public static T? GetParent<T>(DependencyObject d) where T : DependencyObject
        {
            return d is null
                ? default
                : d is T t ? t : d is Window ? null : GetParent<T>(d is Visual ? VisualTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d));
        }

        /// <summary>
        /// 辅助方法：用于查找子控件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T? FindVisualChild<T>(DependencyObject? obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is not null and T)
                {
                    return (T)child;
                }
                else
                {
                    T? childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
    }
}