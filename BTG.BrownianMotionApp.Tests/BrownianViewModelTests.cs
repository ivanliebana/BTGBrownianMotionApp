using BTG.BrownianMotionApp.Services;
using BTG.BrownianMotionApp.ViewModels;

namespace BTG.BrownianMotionApp.Tests
{
    public partial class BrownianViewModelTests
    {
        [Fact]
        public void LineThicknessIsClampedBetween1And10()
        {
            var vm = new BrownianViewModel(new BrownianService());

            vm.LineThickness = 100; // muito alto
            Assert.Equal(10f, vm.ChartDrawable.StrokeSize);

            vm.LineThickness = 0;   // muito baixo
            Assert.Equal(1f, vm.ChartDrawable.StrokeSize);
        }

        [Fact]
        public void TogglingShowAxesRaisesRedraw()
        {
            var vm = new BrownianViewModel(new BrownianService());
            int redraw = 0;
            vm.RequestRedraw += (_, __) => redraw++;

            vm.ShowAxes = !vm.ShowAxes;
            Assert.True(redraw >= 1);
            Assert.Equal(vm.ShowAxes, vm.ChartDrawable.ShowAxes);
        }
    }
}
