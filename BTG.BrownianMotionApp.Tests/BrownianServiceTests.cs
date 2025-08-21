using BTG.BrownianMotionApp.Services;
using System;

namespace BTG.BrownianMotionApp.Tests
{
    public partial class BrownianServiceTests // pode ser a mesma classe do arquivo anterior
    {
        [Fact]
        public void ThrowsWhenNumDaysLessThan2()
        {
            var svc = new BrownianService();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                svc.GenerateBrownianMotion(0.2, 0.01, 100, 1));
        }

        [Fact]
        public void ThrowsWhenInitialPriceNotPositive()
        {
            var svc = new BrownianService();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                svc.GenerateBrownianMotion(0.2, 0.01, 0, 10));
        }

        [Fact]
        public void DifferentSeedsProduceDifferentPaths()
        {
            var svc = new BrownianService();
            var a = svc.GenerateBrownianMotion(0.2, 0.01, 100, 64, seed: 1);
            var b = svc.GenerateBrownianMotion(0.2, 0.01, 100, 64, seed: 2);

            Assert.NotEqual(a, b);
        }
    }
}
