using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Alpha.Converters
{
    public class UniversalComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter is string paramStr && paramStr == "NullToVisible"
                ? (value != null) ? Visibility.Collapsed : Visibility.Visible
                : parameter is string paramStr2 && paramStr2 == "NullToHidden"
                ? (value == null) ? Visibility.Hidden : Visibility.Visible
                : value == null
                ? Visibility.Collapsed
                : parameter == null
                ? value is bool
                    ? (!(bool)value) ? Visibility.Collapsed : Visibility.Visible
                    : value is int
                    ? ((int)value == 0) ? Visibility.Collapsed : Visibility.Visible
                    : value is double
                    ? (!(Math.Abs((double)value) > 0.0001)) ? Visibility.Collapsed : Visibility.Visible
                    : value is string strValue2
                    ? string.IsNullOrEmpty(strValue2) ? Visibility.Collapsed : Visibility.Visible
                    : value.GetType().IsEnum
                    ? value.Equals(Enum.GetValues(value.GetType()).GetValue(0)) ? Visibility.Collapsed : Visibility.Visible
                    : (object)Visibility.Visible
                : (!(((value is not int && value is not double && value is not float) || 1 == 0) ? ((value is bool boolValue && parameter is bool) ? (boolValue == (bool)parameter) : ((value is string strValue && parameter is string paramStr3) ? string.Equals(strValue, paramStr3, StringComparison.InvariantCulture) : ((value.GetType().IsEnum && parameter.GetType().IsEnum) ? value.Equals(parameter) : value.Equals(parameter)))) : (Math.Abs(System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter)) < 0.0001))) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                if (visibility == Visibility.Visible)
                {
                    if (targetType == typeof(bool))
                    {
                        return true;
                    }
                    if (targetType == typeof(int))
                    {
                        return parameter ?? 1;
                    }
                    if (targetType == typeof(double))
                    {
                        return parameter ?? 1.0;
                    }
                    if (targetType == typeof(string))
                    {
                        return parameter?.ToString() ?? string.Empty;
                    }
                    if (targetType.IsEnum && parameter != null)
                    {
                        return Enum.Parse(targetType, parameter.ToString() ?? string.Empty);
                    }
                }
                else
                {
                    if (targetType == typeof(bool))
                    {
                        return false;
                    }
                    if (targetType == typeof(int) || targetType == typeof(double))
                    {
                        return 0;
                    }
                    if (targetType == typeof(string))
                    {
                        return string.Empty;
                    }
                    if (targetType.IsEnum)
                    {
                        Array values;
                        values = Enum.GetValues(targetType);
                        return values.Length <= 0 ? null : values.GetValue(0);
                    }
                }
            }
            throw new InvalidOperationException("ConvertBack只能从Visibility转换。");
        }
    }
}
