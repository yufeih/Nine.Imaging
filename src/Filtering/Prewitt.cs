namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Implements the prewitt edge detection.
    /// </summary>
    public class Prewitt : Matrix2DFilter
    {
        public override double[] KernelX { get; } = new double[]
        {
            -1, -1, -1,
            0,  0,  0,
            1,  1,  1
        };

        public override double[] KernelY { get; } = new double[]
        {
            -1, 0, 1,
            -1, 0, 1,
            -1, 0, 1
        };
    }
}
