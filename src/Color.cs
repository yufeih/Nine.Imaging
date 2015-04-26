namespace Nine.Imaging
{
    using System;
    using System.Text;

    public struct Color : IEquatable<Color>
    {
        public static readonly Color Empty = new Color();
        public static readonly Color Transparent = new Color(0, 255, 255, 255);

        public static readonly Color Black = new Color(255, 0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255, 255);

        public static readonly Color Red = new Color(255, 255, 0, 0);
        public static readonly Color Green = new Color(255, 0, 255, 0);
        public static readonly Color Blue = new Color(255, 0, 0, 255);

        private uint _packedValue;

        public byte B
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 24);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0x00ffffff) | ((uint)value << 24);
            }
        }

        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 16);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xff00ffff) | ((uint)value << 16);
            }
        }
        
        public byte R
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 8);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffff00ff) | ((uint)value << 8);
            }
        }
        
        public byte A
        {
            get
            {
                unchecked
                {
                    return (byte)this._packedValue;
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffffff00) | value;
            }
        }
        
        public Color(int bgra)
        {
            unchecked
            {
                _packedValue = (uint)bgra;
            }
        }

        public Color(byte r, byte g, byte b)
        {
            unchecked
            {
                _packedValue = (uint)(b << 24 | g << 16 | r << 8 | 0xFF);
            }
        }

        public Color(byte a, byte r, byte g, byte b)
        {
            unchecked
            {
                _packedValue = (uint)(b << 24 | g << 16 | r << 8 | a);
            }
        }

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
            return a._packedValue == b._packedValue;
        }
        
        public static bool operator !=(Color a, Color b)
        {
            return a._packedValue != b._packedValue;
        }
        
        public override int GetHashCode()
        {
            return this._packedValue.GetHashCode();
        }
        
        public override bool Equals(object obj)
        {
            return ((obj is Color) && this.Equals((Color)obj));
        }

        public bool Equals(Color other)
        {
            return _packedValue == other._packedValue;
        }
    }
}
