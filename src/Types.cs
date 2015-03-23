// ===============================================================================
// Color.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;

namespace Nine.Imaging
{
    public struct Color
    {
        public byte A, R, G, B;

        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color { A = a, R = r, G = g, B = b };
        }
    }

    public struct Rect
    {
        public double X, Y, Width, Height;
    }

    public struct Point
    {
        public int X, Y;
    }
}
