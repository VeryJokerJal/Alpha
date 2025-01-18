using System.Globalization;
using System.Windows.Data;

namespace Alpha.Converters
{
    public class SubmitToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string status
                ? status.ToUpper() switch
                {
                    "ACTIVE" => "#2E7D32",
                    "UNSUBMITTED" => "#F57C00",
                    _ => "#757575",
                }
                : (object)"#757575";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
