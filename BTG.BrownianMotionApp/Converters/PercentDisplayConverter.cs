using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace BTG.BrownianMotionApp.Converters
{
    /// <summary>
    /// Exibe 0.20 como "20" e, ao digitar "20", envia 0.20 para o binding.
    /// Suporta vírgula ou ponto como separador.
    /// </summary>
    public class PercentDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // VM -> UI
            if (value is double d)
            {
                var pct = d * 100.0;
                // Mostra com até duas casas, respeitando cultura atual
                return Math.Round(pct, 2).ToString("0.##", culture);
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // UI -> VM
            if (value is string s)
            {
                s = s.Trim()
                     .Replace("%", "")
                     .Replace(" ", "");

                // Aceita vírgula ou ponto
                var sInv = s.Replace(',', '.');

                if (double.TryParse(sInv, NumberStyles.Any, CultureInfo.InvariantCulture, out var vInv))
                    return vInv / 100.0;

                // fallback para cultura atual (caso necessário)
                if (double.TryParse(s, NumberStyles.Any, culture, out var v))
                    return v / 100.0;

                return 0.0;
            }

            if (value is double d)
                return d / 100.0;

            return 0.0;
        }
    }
}
