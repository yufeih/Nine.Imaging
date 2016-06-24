namespace Nine.Imaging.Filtering
{
    using System;

    public class GaussianBlur : MatrixFilter
    {
        private double[] filter;
        private double oldVariance;

        public double Variance { get; set; }

        public override double[] Kernel => filter;

        protected override void OnApply()
        {
            var varience = Math.Max(1, Variance);

            if (filter == null || oldVariance != varience)
            {
                int filterSize = (int)(2 * varience) * 2 + 1;

                filter = new double[filterSize * filterSize];

                for (int y = 0; y < filterSize; y++)
                {
                    for (int x = 0; x < filterSize; x++)
                    {
                        int filterX = x - (filterSize / 2);
                        int filterY = y - (filterSize / 2);

                        double v2 = varience * varience;

                        int x2 = filterX * filterX;
                        int y2 = filterY * filterY;

                        double factor = (1.0 / (2 * Math.PI * v2));
                        double exponent = -(x2 + y2) / (2 * v2);

                        filter[x + y * filterSize] = factor * Math.Pow(Math.E, exponent);
                    }
                }

                oldVariance = varience;
            }
        }
    }
}
