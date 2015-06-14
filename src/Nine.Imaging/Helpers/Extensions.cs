// ===============================================================================
// Extensions.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nine.Imaging
{
    /// <summary>
    /// A collection of simple helper extension methods.
    /// </summary>
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
        
        /// <summary>
        /// Determines whether the specified value is between two other
        /// values.
        /// </summary>
        /// <typeparam name="TValue">The type of the values to check.
        /// Must implement <see cref="IComparable"/>.</typeparam>
        /// <param name="value">The value which should be between the other values.</param>
        /// <param name="low">The lower value.</param>
        /// <param name="high">The higher value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is between the other values; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween<TValue>(this TValue value, TValue low, TValue high) where TValue : IComparable
        {
            return (Comparer<TValue>.Default.Compare(low, value) <= 0
                 && Comparer<TValue>.Default.Compare(high, value) >= 0);
        }

        /// <summary>
        /// Arranges the value, so that it is not greater than the high value and
        /// not lower than the low value and returns the result.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="low">The lower value.</param>
        /// <param name="high">The higher value.</param>
        /// <returns>The arranged value.</returns>
        /// <remarks>If the specified lower value is greater than the higher value. The low value
        /// will be returned.</remarks>
        public static TValue RemainBetween<TValue>(this TValue value, TValue low, TValue high) where TValue : IComparable
        {
            TValue result = value;

            if (Comparer<TValue>.Default.Compare(high, low) < 0)
            {
                result = low;
            }

            else if (Comparer<TValue>.Default.Compare(value, low) <= 0)
            {
                result = low;
            }

            else if (Comparer<TValue>.Default.Compare(value, high) >= 0)
            {
                result = high;
            }

            return result;
        }
    }
}
