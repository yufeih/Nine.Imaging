namespace Nine.Imaging
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [StructLayout(LayoutKind.Explicit)]
    public struct Color : IEquatable<Color>
    {
        public static readonly Color Empty = new Color();
        public static readonly Color Transparent = new Color(0, 255, 255, 255);

        public static readonly Color Black = new Color(255, 0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255, 255);

        public static readonly Color Red = new Color(255, 255, 0, 0);
        public static readonly Color Green = new Color(255, 0, 255, 0);
        public static readonly Color Blue = new Color(255, 0, 0, 255);

        [FieldOffset(0)]
        private int bgra;

        [FieldOffset(0)]
        public byte B;
        [FieldOffset(1)]
        public byte G;
        [FieldOffset(2)]
        public byte R;
        [FieldOffset(3)]
        public byte A;

        public Color(int bgra) : this() { this.bgra = bgra; }
        public Color(byte r, byte g, byte b) : this() { R = r; G = g; B = b; A = 255; }
        public Color(byte a, byte r, byte g, byte b) : this() { R = r; G = g; B = b; A = a; }

        public HsbColor ToHsb() => HsbColor.FromRgb(this);
        public static Color FromHsb(HsbColor hsb) => hsb.ToRgb();
        public static Color FromHsb(float hue, float saturation, float brightness)
        {
            return new HsbColor { H = hue, S = saturation, B = brightness, A = 1.0f }.ToRgb();
        }

        public override string ToString()
        {
            var sb = new StringBuilder(32);
            sb.Append("#");

            sb.Append(A.ToString("X2"));
            sb.Append(R.ToString("X2"));
            sb.Append(G.ToString("X2"));
            sb.Append(B.ToString("X2"));

            return sb.ToString();
        }

        public static Color Parse(string value)
        {
            byte a = 255, r, g, b;

            if (value.Contains(","))
            {
                var argb = value.Split(',');
                var i = argb.Length > 3 ? 1 : 0;

                if (i == 1) a = (byte)int.Parse(argb[0]);
                r = (byte)int.Parse(argb[i + 0]);
                g = (byte)int.Parse(argb[i + 1]);
                b = (byte)int.Parse(argb[i + 2]);
            }
            else
            {
                var i = value.StartsWith("#") ? 1 : 0;

                if (value.Length - i == 3)
                {
                    r = g = b = (byte)(Hex(value[i + 0]) * 16 + Hex(value[i + 1]));
                }
                else if (value.Length - i == 6)
                {
                    r = (byte)(Hex(value[i + 0]) * 16 + Hex(value[i + 1]));
                    g = (byte)(Hex(value[i + 2]) * 16 + Hex(value[i + 3]));
                    b = (byte)(Hex(value[i + 4]) * 16 + Hex(value[i + 5]));
                }
                else
                {
                    a = (byte)(Hex(value[i + 0]) * 16 + Hex(value[i + 1]));
                    r = (byte)(Hex(value[i + 2]) * 16 + Hex(value[i + 3]));
                    g = (byte)(Hex(value[i + 4]) * 16 + Hex(value[i + 5]));
                    b = (byte)(Hex(value[i + 6]) * 16 + Hex(value[i + 7]));
                }
            }

            return new Color(a, r, g, b);
        }

        private static int Hex(char c)
        {
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            if (c >= 'a' && c <= 'z') return c - 'a' + 10;
            if (c >= '0' && c <= '9') return c - '0';

            throw new ArgumentOutOfRangeException($"{ c } is not a valid for hex number");
        }

        public static bool operator ==(Color a, Color b)
        {
            return a.bgra == b.bgra;
        }
        
        public static bool operator !=(Color a, Color b)
        {
            return a.bgra != b.bgra;
        }
        
        public override int GetHashCode()
        {
            return this.bgra.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
            return ((obj is Color) && this.Equals((Color)obj));
        }

        public bool Equals(Color other)
        {
            return bgra == other.bgra;
        }
    }
}
