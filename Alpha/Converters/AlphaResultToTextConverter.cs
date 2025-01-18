using System.Globalization;
using System.Windows.Data;
using Alpha.Models;

namespace Alpha.Converters
{
    public class AlphaResultToTextConverter : IValueConverter
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
                        string? result = check.Result;
                        return result != null && (result.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                    })
                        ? "已通过"
                        : (object)"未通过";
                }
            }
            return "未知状态";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
