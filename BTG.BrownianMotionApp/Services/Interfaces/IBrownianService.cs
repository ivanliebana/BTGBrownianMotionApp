namespace BTG.BrownianMotionApp.Services.Interfaces
{
    public interface IBrownianService
    {
        double[] GenerateBrownianMotion(double sigma, double mean, double initialPrice, int numDays, int? seed = null);
        double[][] GenerateMultiple(double sigma, double mean, double initialPrice, int numDays, int simulations, int? seed = null);
    }
}
