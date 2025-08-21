using BTG.BrownianMotionApp.Converters;
using System.Globalization;

namespace BTG.BrownianMotionApp.Tests
{
    public partial class PercentDisplayConverterTests
    {
        [Fact]
        public void ConvertBack_AcceptsDotAsDecimalSeparator()
        {
            var conv = new PercentDisplayConverter();
            var pt = CultureInfo.GetCultureInfo("pt-BR");

            var val = (double)conv.ConvertBack("27.8", typeof(double), null, pt);
            Assert.InRange(val, 0.278 - 1e-9, 0.278 + 1e-9);
        }

        [Fact]
        public void ConvertBack_InvalidReturnsZero()
        {
            var conv = new PercentDisplayConverter();
            var pt = CultureInfo.GetCultureInfo("pt-BR");

            var val = (double)conv.ConvertBack("abc", typeof(double), null, pt);
            Assert.Equal(0.0, val);
        }
    }
}
