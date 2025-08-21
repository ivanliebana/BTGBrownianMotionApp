using BTG.BrownianMotionApp.ViewModels;
using Microsoft.Maui.Controls;

namespace BTG.BrownianMotionApp.Views;

public partial class BrownianPage : ContentPage
{
    private readonly BrownianViewModel _vm;

    public BrownianPage(BrownianViewModel vm) // VM por DI
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;

        // Liga o desenho do gráfico
        ChartView.Drawable = vm.ChartDrawable;

        // Redesenha quando o VM avisar
        _vm.RequestRedraw += (_, __) => ChartView.Invalidate();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        bool narrow = width < 900;

        if (narrow)
        {
            // Gráfico ocupa 2 colunas na primeira linha
            Grid.SetRow(ChartCard, 0);
            Grid.SetColumn(ChartCard, 0);
            Grid.SetColumnSpan(ChartCard, 2);

            // Painel vai para a linha de baixo, ocupando 2 colunas
            Grid.SetRow(ParamsScroll, 1);
            Grid.SetColumn(ParamsScroll, 0);
            Grid.SetColumnSpan(ParamsScroll, 2);
        }
        else
        {
            // Layout em 2 colunas: gráfico à esquerda
            Grid.SetRow(ChartCard, 0);
            Grid.SetColumn(ChartCard, 0);
            Grid.SetColumnSpan(ChartCard, 1);

            // Painel à direita na mesma linha
            Grid.SetRow(ParamsScroll, 0);
            Grid.SetColumn(ParamsScroll, 1);
            Grid.SetColumnSpan(ParamsScroll, 1);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.IsEmpty)
        {
            if (_vm.Steps < 2) _vm.Steps = 50;
            if (_vm.Simulations < 1) _vm.Simulations = 1;
            if (_vm.InitialPrice <= 0) _vm.InitialPrice = 100;
            if (_vm.Volatility <= 0) _vm.Volatility = 0.2;
            _vm.RunCommand.Execute(null);
        }
    }
}