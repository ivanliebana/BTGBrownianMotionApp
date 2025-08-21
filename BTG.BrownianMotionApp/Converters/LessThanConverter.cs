using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BTG.BrownianMotionApp.Converters
{
    /// <summary>
    /// Retorna true se (double)value < (double)parameter.
    /// Usado para DataTriggers de responsividade.
    /// </summary>
    public class LessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v && parameter is not null
                && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
                return v < p;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
