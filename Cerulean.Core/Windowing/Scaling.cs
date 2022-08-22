using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cerulean.Common;

namespace Cerulean.Core
{
    public static class Scaling
    {
        public static double GetDpiScaledValue(Window? window, double value)
        {
            if (window == null)
                return value;
            var dpi = window.GraphicsContext?.GetCurrentDisplayDpi() ?? 96;
            var scale = window.AutoScale ? dpi / 96f : 1f;

            return value * scale;
        }

        public static int GetDpiScaledValue(Window? window, int value)
        {
            if (window == null)
                return value;
            var dpi = window.GraphicsContext?.GetCurrentDisplayDpi() ?? 96;
            var scale = window.AutoScale ? dpi / 96f : 1f;

            return (int)Math.Floor(value * scale);
        }

        public static Size GetDpiScaledValue(Window? window, Size value)
        {
            if (window == null)
                return value;
            return new Size()
            {
                W = GetDpiScaledValue(window, value.W),
                H = GetDpiScaledValue(window, value.H)
            };
        }

        public static Size? GetDpiScaledValue(Window? window, Size? value)
        {
            if (window == null || value == null)
                return value;
            return new Size()
            {
                W = GetDpiScaledValue(window, value.Value.W),
                H = GetDpiScaledValue(window, value.Value.H)
            };
        }

        public static (int, int) GetDpiScaledValue(Window? window, (int, int) value)
        {
            if (window == null)
                return value;
            return (GetDpiScaledValue(window, value.Item1),
                    GetDpiScaledValue(window, value.Item2));
        }
    }
}
