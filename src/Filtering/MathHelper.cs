namespace Nine.Imaging.Filtering
{
    using System;
    using System.Collections.Generic;

    static class MathHelper
    {
        public static double Clamp(this double value, double low, double high)
        {
            if (value < low) return low;
            if (value > high) return high;

            return value;
        }

        public static int Clamp(this int value, int low, int high)
        {
            if (value < low) return low;
            if (value > high) return high;

            return value;
        }
    }
}
