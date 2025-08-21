namespace BTG.BrownianMotionApp.Models
{
    public record BrownianParams(
            double InitialPrice,
            double MeanReturn,
            double Volatility,
            double TimeHorizon,
            int Steps,
            int Simulations
    );
}
