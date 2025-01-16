using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Alpha.Converters
{
    public class CheckResultColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string result
                ? result.ToUpper() switch
                {
                    "PASS" => new SolidColorBrush(Color.FromRgb(40, 167, 69)),
                    "FAIL" => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                    "PENDING" => new SolidColorBrush(Color.FromRgb(byte.MaxValue, 193, 7)),
                    _ => new SolidColorBrush(Color.FromRgb(108, 117, 125)),
                }
                : (object)new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
