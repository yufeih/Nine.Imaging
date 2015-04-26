namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Implements the sobel filter edge detection.
    /// </summary>
    public class Sobel : Matrix2DFilter
    {
        public override double[] KernelX { get; } = new double[]
        {
            -1, -2, -1,
            0,  0,  0,
            1,  2,  1
        };

        public override double[] KernelY { get; } = new double[]
        {
            -1, 0, 1,
            -2, 0, 2,
            -1, 0, 1
        };
    }
}
