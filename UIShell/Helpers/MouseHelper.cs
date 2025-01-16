﻿using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace UIShell.Helpers
{
    internal static class MouseHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct PointWrap
        {
            public int X;
            public int Y;
        }

        private static BOOL GetPointWrap(out PointWrap point)
        {
            unsafe
            {
                fixed (PointWrap* ptr = &point)
                {
                    return PInvoke.GetCursorPos((System.Drawing.Point*)ptr);
                }
            }
        }

        public static Point GetMousePosition()
        {
            return GetPointWrap(out PointWrap point) ? new Point(point.X, point.Y) : new Point();
        }
    }
}
