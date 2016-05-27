namespace Nine.Imaging
{
    using System;
    using System.Collections.Generic;
    
    static class Extensions
    {
        /// <summary>
        /// Converts byte array to a new array where each value in the original array is represented 
        /// by a the specified number of bits.
        /// </summary>
        /// <param name="bytes">The bytes to convert from. Cannot be null.</param>
        /// <param name="bits">The number of bits per value.</param>
        /// <returns>The resulting byte array. Is never null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="bits"/> is greater or equals than zero.</exception>
        public static byte[] ToArrayByBitsLength(this byte[] bytes, int bits)
        {
            byte[] result = null;

            if (bits < 8)
            {
                result = new byte[bytes.Length * 8 / bits];

                int factor = (int)Math.Pow(2, bits) - 1;
                int mask = (0xFF >> (8 - bits));
                int resultOffset = 0;

                for (int i = 0; i < bytes.Length; i++)
                {
                    for (int shift = 0; shift < 8; shift += bits)
                    {
                        int colorIndex = (((bytes[i]) >> (8 - bits - shift)) & mask) * (255 / factor);

                        result[resultOffset] = (byte)colorIndex;

                        resultOffset++;
                    }

                }
            }
            else
            {
                result = bytes;
            }

            return result;
        }
        
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
