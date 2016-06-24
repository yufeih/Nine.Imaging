namespace Nine.Imaging.Filtering
{
    using System;

    public class CropCircle : ParallelImageFilter
    {
        private static readonly double[] offsetsX = new[] { 0.0, 0.0, 0.5, 1.0, 1.0 };
        private static readonly double[] offsetsY = new[] { 0.0, 1.0, 0.5, 1.0, 0.0 };

        public double Radius { get; set; } = -1;

        protected override void Apply(Image target, Image source, Rectangle rectangle, int startY, int endY)
        {
            var radius = Radius >= 0 ? Radius : Math.Min(rectangle.Width, rectangle.Height) * 0.5;
            var radiusSq = radius * radius;
            var centerX = (rectangle.Left + rectangle.Right) * 0.5;
            var centerY = (rectangle.Top + rectangle.Bottom) * 0.5;
            var count = offsetsX.Length;
            var inc = 1.0 / count;

            for (int y = startY; y < endY; y++)
            {
                for (int x = rectangle.X; x < rectangle.Right; x++)
                {
                    var a = 0.0;
                    var color = source[x, y];

                    for (var i = 0; i < count; i++)
                    {
                        var cx = (x + offsetsX[i] - centerX);
                        var cy = (y + offsetsY[i] - centerY);

                        if (radiusSq > cx * cx + cy * cy)
                        {
                            a += inc;
                        }
                    }

                    if (a < 1.0)
                    {
                        color = new Color(
                            (byte)(color.R), 
                            (byte)(color.G),
                            (byte)(color.B),
                            (byte)(a * color.A));
                    }

                    target[x, y] = color;
                }
            }
        }
    }
}
