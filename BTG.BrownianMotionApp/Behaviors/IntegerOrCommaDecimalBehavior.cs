using Microsoft.Maui.Controls;
using System;
using System.Text.RegularExpressions;

namespace BTG.BrownianMotionApp.Behaviors
{
    /// <summary>
    /// Permite números no formato inteiro "27" ou decimal com vírgula "27,8".
    /// - Troca ponto por vírgula automaticamente.
    /// - Suporta digitação parcial (ex.: "27," durante a edição).
    /// - Pode limitar quantidade de dígitos antes/depois da vírgula.
    /// </summary>
    public class IntegerOrCommaDecimalBehavior : Behavior<Entry>
    {
        // Configurações
        public bool AllowNegative { get; set; } = false;
        public int? MaxIntegerDigits { get; set; } = null;     // ex.: 5 => até 99999
        public int? MaxFractionDigits { get; set; } = null;    // ex.: 1 => "27,8"
        public bool ReplaceDotWithComma { get; set; } = true;  // 27.8 => 27,8
        public bool AllowEmpty { get; set; } = true;           // deixa apagar tudo enquanto edita

        // Padrões
        private static readonly Regex _partial = new(@"^-?\d*(,\d*)?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex _full = new(@"^-?\d+(,\d+)?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private bool _busy;

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnTextChanged;
            bindable.Completed += OnCompleted;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnTextChanged;
            bindable.Completed -= OnCompleted;
        }

        private void OnTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_busy) return;
            if (sender is not Entry entry) return;

            var newText = e.NewTextValue ?? string.Empty;

            // Normaliza
            var normalized = newText.Trim();
            if (ReplaceDotWithComma && normalized.Contains('.'))
                normalized = normalized.Replace('.', ',');

            // Vazio
            if (string.IsNullOrEmpty(normalized))
            {
                if (AllowEmpty) return;     // deixa em branco
                normalized = "0";
            }

            // Padrão parcial (permite "27," enquanto digita)
            if (!_partial.IsMatch(normalized))
            {
                Revert(entry, e.OldTextValue);
                return;
            }

            // Negativos?
            if (!AllowNegative && normalized.StartsWith("-", StringComparison.Ordinal))
            {
                Revert(entry, e.OldTextValue);
                return;
            }

            // Limites de dígitos
            var parts = normalized.Split(',');
            var intPart = parts.Length > 0 ? parts[0].Replace("-", "") : "";
            var fracPart = parts.Length > 1 ? parts[1] : null;

            if (MaxIntegerDigits.HasValue && intPart.Length > MaxIntegerDigits.Value)
            {
                Revert(entry, e.OldTextValue);
                return;
            }

            if (fracPart is not null && MaxFractionDigits.HasValue && fracPart.Length > MaxFractionDigits.Value)
            {
                Revert(entry, e.OldTextValue);
                return;
            }

            // Aplica normalização sem loop
            if (normalized != newText)
            {
                _busy = true;
                entry.Text = normalized;
                _busy = false;
            }
        }

        private void OnCompleted(object? sender, EventArgs e)
        {
            if (sender is not Entry entry) return;

            var t = entry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(t)) return;

            // Remove vírgula final solta
            if (t.EndsWith(",", StringComparison.Ordinal))
                t = t.TrimEnd(',');

            // Normaliza zeros à esquerda
            bool neg = t.StartsWith("-", StringComparison.Ordinal);
            if (neg) t = t[1..];

            var parts = t.Split(',');
            var intPart = parts[0];
            var fracPart = parts.Length > 1 ? parts[1] : null;

            intPart = string.IsNullOrEmpty(intPart) ? "0" : intPart.TrimStart('0');
            if (intPart == "") intPart = "0";

            // Enforca limites, se houver
            if (MaxIntegerDigits.HasValue && intPart.Length > MaxIntegerDigits.Value)
                intPart = intPart[^MaxIntegerDigits.Value..];

            if (fracPart is not null && MaxFractionDigits.HasValue && fracPart.Length > MaxFractionDigits.Value)
                fracPart = fracPart[..MaxFractionDigits.Value];

            var final = (neg && AllowNegative ? "-" : "") + intPart;
            if (!string.IsNullOrEmpty(fracPart))
                final += "," + fracPart;

            // Valida final
            if (!_full.IsMatch(final))
                final = (neg && AllowNegative ? "-" : "") + intPart; // fallback: inteiro

            if (entry.Text != final)
            {
                _busy = true;
                entry.Text = final;
                _busy = false;
            }
        }

        private void Revert(Entry entry, string? oldText)
        {
            _busy = true;
            entry.Text = oldText ?? string.Empty;
            _busy = false;
        }
    }
}
