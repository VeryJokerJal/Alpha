using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Alpha.Converters
{
    public class LevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string level
                ? level.ToUpper() switch
                {
                    "GOLD" => new SolidColorBrush(Color.FromRgb(212, 175, 55)),    // 金色 #FFD4AF37
                    "SILVER" => new SolidColorBrush(Color.FromRgb(192, 192, 192)), // 银色 #FFC0C0C0
                    "BRONZE" => new SolidColorBrush(Color.FromRgb(205, 127, 50)),  // 铜色 #FFCD7F32
                    _ => new SolidColorBrush(Color.FromRgb(192, 192, 192))         // 默认银色
                }
                : (object)new SolidColorBrush(Color.FromRgb(192, 192, 192));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
