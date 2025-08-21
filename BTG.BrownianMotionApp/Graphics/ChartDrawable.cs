using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTG.BrownianMotionApp.Graphics
{
    public class ChartDrawable : IDrawable
    {
        private readonly List<double[]> _series = new();

        public bool ShowAxes { get; set; } = true;
        public bool IsEmpty => _series.Count == 0;

        // Personalização
        public float StrokeSize { get; set; } = 2f;
        public string LineStyle { get; set; } = "Sólida"; // "Sólida" | "Tracejada" | "Pontilhada"
        public Color BaseColor { get; set; } = Color.FromArgb("#9FA0FF");

        public void SetSeries(IEnumerable<double[]> series)
        {
            _series.Clear();
            _series.AddRange(series.Where(s => s != null && s.Length >= 2));
        }

        public void Clear() => _series.Clear();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            if (_series.Count == 0)
            {
                canvas.RestoreState();
                return;
            }

            // Layout
            float left = 60, right = 20, top = 20, bottom = 50;
            var plot = new RectF(dirtyRect.Left + left, dirtyRect.Top + top,
                                 dirtyRect.Width - left - right, dirtyRect.Height - top - bottom);

            // Segurança
            int maxLen = _series.Max(s => s?.Length ?? 0);
            if (maxLen < 2)
            {
                canvas.RestoreState();
                return;
            }

            // Min/Max global
            double minY = _series.Min(s => s.Min());
            double maxY = _series.Max(s => s.Max());
            if (Math.Abs(maxY - minY) < 1e-9) { maxY += 1; minY -= 1; }

            // Eixos e grid
            if (ShowAxes)
                DrawAxes(canvas, plot, minY, maxY, maxLen);

            // Estilo
            canvas.StrokeSize = StrokeSize;
            switch (LineStyle)
            {
                case "Tracejada": canvas.StrokeDashPattern = new float[] { 6, 4 }; break;
                case "Pontilhada": canvas.StrokeDashPattern = new float[] { 2, 4 }; break;
                default: canvas.StrokeDashPattern = null; break;
            }

            // Paleta baseada na cor
            var palette = BuildPalette(BaseColor);

            // Desenho das séries
            for (int i = 0; i < _series.Count; i++)
            {
                var s = _series[i];
                canvas.StrokeColor = palette[i % palette.Count];

                PointF? last = null;
                for (int x = 0; x < s.Length; x++)
                {
                    float px = plot.Left + (float)x / (maxLen - 1) * plot.Width;
                    float py = ValueToY((float)s[x], plot, (float)minY, (float)maxY);
                    var pt = new PointF(px, py);
                    if (last.HasValue) canvas.DrawLine(last.Value, pt);
                    last = pt;
                }
            }

            canvas.RestoreState();
        }

        private static float ValueToY(float value, RectF plot, float min, float max)
        {
            float t = (value - min) / (max - min);
            return plot.Bottom - t * plot.Height;
        }

        private static void DrawAxes(ICanvas canvas, RectF plot, double minY, double maxY, int maxLen)
        {
            canvas.FontColor = Colors.White;

            canvas.StrokeSize = 1;
            canvas.StrokeColor = Colors.Gray;

            // Bordas do plot
            canvas.DrawRectangle(plot);

            // --- Ticks Y ---
            const float yTickLabelWidth = 56f;
            float yTickLabelX = plot.Left - yTickLabelWidth - 6; // área reservada p/ números

            int yTicks = 5;
            for (int i = 0; i <= yTicks; i++)
            {
                float t = (float)i / yTicks;
                float y = plot.Bottom - t * plot.Height;

                canvas.StrokeColor = Colors.LightGray;
                canvas.DrawLine(plot.Left, y, plot.Right, y);

                canvas.StrokeColor = Colors.Gray;
                double v = minY + t * (maxY - minY);
                canvas.DrawString(v.ToString("F2"),
                                  yTickLabelX, y - 8, yTickLabelWidth, 16,
                                  HorizontalAlignment.Right, VerticalAlignment.Center);
            }

            // --- Ticks X ---
            int xTicks = 6;
            for (int i = 0; i <= xTicks; i++)
            {
                float t = (float)i / xTicks;
                float x = plot.Left + t * plot.Width;

                canvas.StrokeColor = Colors.LightGray;
                canvas.DrawLine(x, plot.Top, x, plot.Bottom);

                canvas.StrokeColor = Colors.Gray;
                int idx = (int)Math.Round(t * (maxLen - 1));
                canvas.DrawString(idx.ToString(),
                                  x - 12, plot.Bottom + 4, 24, 16,
                                  HorizontalAlignment.Center, VerticalAlignment.Top);
            }

            // --- Rótulos dos eixos ---
            // X (embaixo, centralizado)
            canvas.DrawString("Tempo",
                              plot.Center.X - 20, plot.Bottom + 20, 60, 20,
                              HorizontalAlignment.Center, VerticalAlignment.Top);

            // Y (vertical, imediatamente à esquerda do retângulo do plot)
            canvas.SaveState();
            // posicione o eixo Y 10px à esquerda da borda do plot
            canvas.Translate(plot.Left - 10, plot.Center.Y);
            canvas.Rotate(-90);
            // centraliza no sentido do eixo Y; largura = altura do plot
            canvas.DrawString("Preço",
                              -plot.Height / 2, -12, plot.Height, 24,
                              HorizontalAlignment.Center, VerticalAlignment.Center);
            canvas.RestoreState();
        }

        private static List<Color> BuildPalette(Color baseColor)
        {
            // 8 variações simples da cor base
            static Color Clamp(Color c, float f, float a) =>
                Color.FromRgba(Math.Clamp(c.Red * f, 0, 1),
                               Math.Clamp(c.Green * f, 0, 1),
                               Math.Clamp(c.Blue * f, 0, 1),
                               a);

            return new List<Color>
            {
                baseColor,
                Color.FromRgba(baseColor.Red, baseColor.Green, baseColor.Blue, 0.9f),
                Clamp(baseColor, 0.9f, baseColor.Alpha),
                Clamp(baseColor, 0.8f, baseColor.Alpha),
                Color.FromRgba(baseColor.Red, baseColor.Green, baseColor.Blue, 0.7f),
                Colors.White.WithAlpha(0.7f),
                Colors.Gray,
                Colors.Crimson.WithAlpha(0.9f)
            };
        }
    }
}
