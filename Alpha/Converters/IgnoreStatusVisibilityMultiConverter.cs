using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Alpha.Models;

namespace Alpha.Converters
{
    public class IgnoreStatusVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is IgnoreStatus status && values[1] is SimulationCompletedData param)
            {
                if ((status & IgnoreStatus.Stop) == IgnoreStatus.Stop)
                {
                    AlphaResponse? alphaResult;
                    alphaResult = param.AlphaResult;
                    if (alphaResult != null && (alphaResult.Is?.Checks?.Any(delegate (AlphaCheck check)
                    {
                        string? result2;
                        result2 = check.Result;
                        return result2 != null && (result2.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                    })).GetValueOrDefault())
                    {
                        return Visibility.Collapsed;
                    }
                }
                if ((status & IgnoreStatus.Pass) == IgnoreStatus.Pass)
                {
                    AlphaResponse? alphaResult2;
                    alphaResult2 = param.AlphaResult;
                    if (alphaResult2 != null && alphaResult2.Is?.Checks?.Any(delegate (AlphaCheck check)
                    {
                        string? result;
                        result = check.Result;
                        return result != null && (result.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                    }) == false)
                    {
                        return Visibility.Collapsed;
                    }
                }
                if ((status & IgnoreStatus.Failure) == IgnoreStatus.Failure)
                {
                    string? status2;
                    status2 = param.Status;
                    if (status2 != null && (status2.ToUpper()?.Equals("FAIL", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                    {
                        return Visibility.Collapsed;
                    }
                }
                if ((status & IgnoreStatus.Warning) == IgnoreStatus.Warning)
                {
                    string? status3;
                    status3 = param.Status;
                    if (status3 != null && (status3.ToUpper()?.Equals("WARNING", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                    {
                        return Visibility.Collapsed;
                    }
                }
                if ((status & IgnoreStatus.Complete) == IgnoreStatus.Complete)
                {
                    string? status4;
                    status4 = param.Status;
                    if (status4 != null && (status4.ToUpper()?.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                    {
                        return Visibility.Collapsed;
                    }
                }
                if ((status & IgnoreStatus.Error) == IgnoreStatus.Error)
                {
                    string? status5;
                    status5 = param.Status;
                    if (status5 != null && (status5.ToUpper()?.Equals("ERROR", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
