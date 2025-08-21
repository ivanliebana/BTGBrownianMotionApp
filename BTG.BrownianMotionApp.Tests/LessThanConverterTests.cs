using BTG.BrownianMotionApp.Converters;
using System.Globalization;

namespace BTG.BrownianMotionApp.Tests
{
    public class LessThanConverterTests
    {
        [Theory]
        [InlineData(800.0, 900.0, true)]
        [InlineData(1000.0, 900.0, false)]
        public void WorksAsExpected(double value, double param, bool expected)
        {
            var c = new LessThanConverter();
            var result = (bool)c.Convert(value, typeof(bool), param, CultureInfo.InvariantCulture);
            Assert.Equal(expected, result);
        }
    }
}
