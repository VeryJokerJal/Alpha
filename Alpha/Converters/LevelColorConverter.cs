using System.Globalization;
using System.Windows;
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
             "GRANDMASTER" => new LinearGradientBrush
             {
                 StartPoint = new Point(0, 0),
                 EndPoint = new Point(1, 1),
                 GradientStops = new GradientStopCollection
                 {
                new GradientStop(Color.FromRgb(255, 192, 203), 0.0),   // 粉红
                new GradientStop(Color.FromRgb(255, 182, 193), 0.5),   // 浅粉红
                new GradientStop(Color.FromRgb(255, 160, 200), 1.0)    // 中粉红
                 }
             },
             "MASTER" => new LinearGradientBrush
             {
                 StartPoint = new Point(0, 0),
                 EndPoint = new Point(1, 1),
                 GradientStops = new GradientStopCollection
                 {
                new GradientStop(Color.FromRgb(230, 190, 255), 0.0),   // 浅紫
                new GradientStop(Color.FromRgb(221, 160, 255), 0.5),   // 淡紫
                new GradientStop(Color.FromRgb(200, 150, 255), 1.0)    // 中紫
                 }
             },
             "EXPERT" => new LinearGradientBrush
             {
                 StartPoint = new Point(0, 0),
                 EndPoint = new Point(1, 1),
                 GradientStops = new GradientStopCollection
                 {
                new GradientStop(Color.FromRgb(135, 206, 250), 0.0),   // 浅天蓝
                new GradientStop(Color.FromRgb(176, 224, 230), 0.5),   // 粉蓝
                new GradientStop(Color.FromRgb(173, 216, 230), 1.0)    // 淡蓝
                 }
             },
             "GOLD" => new LinearGradientBrush
             {
                 StartPoint = new Point(0, 0),
                 EndPoint = new Point(1, 1),
                 GradientStops = new GradientStopCollection
                 {
                new GradientStop(Color.FromRgb(255, 223, 0), 0.0),     // 明亮金
                new GradientStop(Color.FromRgb(255, 215, 0), 0.5),     // 金色
                new GradientStop(Color.FromRgb(255, 200, 0), 1.0)      // 浅金
                 }
             },
             "SILVER" => new SolidColorBrush(Color.FromRgb(192, 192, 192)),      // 银色
             "BRONZE" => new SolidColorBrush(Color.FromRgb(205, 127, 50)),       // 铜色
             _ => new SolidColorBrush(Color.FromRgb(192, 192, 192))              // 默认银色
         }
         : new SolidColorBrush(Color.FromRgb(192, 192, 192));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
