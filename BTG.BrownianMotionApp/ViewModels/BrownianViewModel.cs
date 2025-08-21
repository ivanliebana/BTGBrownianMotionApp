using BTG.BrownianMotionApp.Graphics;
using BTG.BrownianMotionApp.Services.Interfaces;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BTG.BrownianMotionApp.ViewModels
{
    public class BrownianViewModel : INotifyPropertyChanged
    {
        private readonly IBrownianService _service;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? RequestRedraw;

        public ChartDrawable ChartDrawable { get; } = new();

        // Estado padrão
        private double _initialPrice = 100.0;
        private double _meanReturn = 0.01;  // 1% diário
        private double _volatility = 0.20;  // 20% diário
        private int _steps = 252;
        private int _simulations = 1;
        private bool _showAxes = true;

        // Personalização
        private double _lineThickness = 2;
        private string _selectedLineStyle = "Sólida";
        private string _selectedColorName = "Lilás";

        public BrownianViewModel(IBrownianService service)
        {
            _service = service;

            // Defaults visuais
            ChartDrawable.ShowAxes = _showAxes;
            ChartDrawable.StrokeSize = (float)_lineThickness;
            ChartDrawable.LineStyle = _selectedLineStyle;
            ChartDrawable.BaseColor = Color.FromArgb("#9FA0FF"); // Lilás
        }

        // Propriedades de binding
        public double InitialPrice { get => _initialPrice; set => Set(ref _initialPrice, value); }
        public double MeanReturn { get => _meanReturn; set => Set(ref _meanReturn, value); }
        public double Volatility { get => _volatility; set => Set(ref _volatility, value); }
        public int Steps { get => _steps; set => Set(ref _steps, value); }
        public int Simulations { get => _simulations; set => Set(ref _simulations, value); }

        public bool ShowAxes
        {
            get => _showAxes;
            set
            {
                if (Set(ref _showAxes, value))
                {
                    ChartDrawable.ShowAxes = value;
                    RequestRedraw?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsEmpty => ChartDrawable.IsEmpty;

        // Personalização para binders
        public double LineThickness
        {
            get => _lineThickness;
            set
            {
                if (Set(ref _lineThickness, value))
                {
                    ChartDrawable.StrokeSize = (float)Math.Max(1, Math.Min(10, value));
                    RequestRedraw?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string SelectedLineStyle
        {
            get => _selectedLineStyle;
            set
            {
                if (Set(ref _selectedLineStyle, value))
                {
                    ChartDrawable.LineStyle = value; // "Sólida" | "Tracejada" | "Pontilhada"
                    RequestRedraw?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public string SelectedColorName
        {
            get => _selectedColorName;
            set
            {
                if (Set(ref _selectedColorName, value))
                {
                    ChartDrawable.BaseColor = value switch
                    {
                        "Lilás" => Color.FromArgb("#9FA0FF"),
                        "Laranja" => Colors.Orange,
                        "Ciano" => Colors.Teal,
                        "Verde" => Colors.Green,
                        "Vermelho" => Colors.Red,
                        _ => Colors.Blue
                    };
                    RequestRedraw?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IList<string> LineStyleOptions { get; } = new List<string> { "Sólida", "Tracejada", "Pontilhada" };
        public IList<string> ColorOptions { get; } = new List<string> { "Lilás", "Laranja", "Ciano", "Verde", "Vermelho" };

        // Commands
        public ICommand RunCommand => new Command(Run);
        public ICommand ClearCommand => new Command(Clear);

        private void Run()
        {
            try
            {
                // Ordem nova: (sigma, mean, initialPrice, numDays, simulations)
                var sims = _service.GenerateMultiple(
                    Volatility,
                    MeanReturn,
                    InitialPrice,
                    Steps,
                    Simulations
                );

                ChartDrawable.SetSeries(sims);
                OnPropertyChanged(nameof(IsEmpty));
                RequestRedraw?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void Clear()
        {
            ChartDrawable.Clear();
            OnPropertyChanged(nameof(IsEmpty));
            RequestRedraw?.Invoke(this, EventArgs.Empty);
        }

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
                return true;
            }
            return false;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
