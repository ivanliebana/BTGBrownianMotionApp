using BTG.BrownianMotionApp.Graphics;

namespace BTG.BrownianMotionApp.Tests
{
    public class ChartDrawableTests
    {
        [Fact]
        public void SetSeries_IgnoresShortOrNullAndSetsIsEmpty()
        {
            var drawable = new ChartDrawable();

            Assert.True(drawable.IsEmpty);

            // contém séries inválidas e válidas
            var series = new double[][]
            {
            new double[]{ 100 },                 // inválida (<2)
            null!,                                // inválida
            new double[]{ 100, 101, 99, 102 }    // válida
            };

            drawable.SetSeries(series);
            Assert.False(drawable.IsEmpty);

            drawable.Clear();
            Assert.True(drawable.IsEmpty);
        }
    }
}
