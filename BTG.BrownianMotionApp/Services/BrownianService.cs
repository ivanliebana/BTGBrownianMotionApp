using BTG.BrownianMotionApp.Services.Interfaces;
using System;

namespace BTG.BrownianMotionApp.Services
{
    /// <summary>
    /// GBM simplificado por dia:
    /// retorno = mean + sigma * Z  (Z ~ N(0,1))
    /// preço[t] = preço[t-1] * exp(retorno)
    /// </summary>
    public class BrownianService : IBrownianService
    {
        public double[] GenerateBrownianMotion(double sigma, double mean, double initialPrice, int numDays, int? seed = null)
        {
            if (initialPrice <= 0) throw new ArgumentOutOfRangeException(nameof(initialPrice));
            if (numDays < 2) throw new ArgumentOutOfRangeException(nameof(numDays));

            var prices = new double[numDays];
            prices[0] = initialPrice;

            var rand = seed.HasValue ? new Random(seed.Value) : new Random();

            for (int i = 1; i < numDays; i++)
            {
                // Box–Muller
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

                double retorno = mean + sigma * z;     // mean/sigma diários
                prices[i] = prices[i - 1] * Math.Exp(retorno);
            }

            return prices;
        }

        public double[][] GenerateMultiple(double sigma, double mean, double initialPrice, int numDays, int simulations, int? seed = null)
        {
            if (simulations < 1) throw new ArgumentOutOfRangeException(nameof(simulations));

            var results = new double[simulations][];
            var rng = seed.HasValue ? new Random(seed.Value) : new Random();

            for (int i = 0; i < simulations; i++)
            {
                int s = rng.Next();
                results[i] = GenerateBrownianMotion(sigma, mean, initialPrice, numDays, s);
            }

            return results;
        }
    }
}