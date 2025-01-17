using System.Globalization;
using System.Windows.Data;

namespace Alpha.Converters
{
    public class ProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 4 &&
                values[0] is double bottom &&
                values[1] is double top &&
                values[2] is double current &&
                values[3] is double totalWidth)
            {
                double progress = (current - bottom) / (top - bottom);
                return Math.Max(0, Math.Min(1, progress)) * totalWidth;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
