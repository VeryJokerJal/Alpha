using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Alpha.Models;

namespace Alpha.Converters
{
    public class AlphaResultToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlphaResponse response)
            {
                List<AlphaCheck>? checks;
                checks = response.Is?.Checks;
                if (checks != null)
                {
                    return !checks.Any(delegate (AlphaCheck check)
                    {
                        string? result;
                        result = check.Result;
                        return result != null && (result.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                    })
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"))
                        : (object)new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
