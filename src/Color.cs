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

        public float Brightness
        {
            get
            {
                float r = (float)R / 255.0f;
                float g = (float)G / 255.0f;
                float b = (float)B / 255.0f;

                float max, min;

                max = r; min = r;

                if (g > max) max = g;
                if (b > max) max = b;

                if (g < min) min = g;
                if (b < min) min = b;

                return (max + min) / 2;
            }
        }
        
        public float Hue
        {
            get
            {
                if (R == G && G == B)
                    return 0;

                float r = (float)R / 255.0f;
                float g = (float)G / 255.0f;
                float b = (float)B / 255.0f;

                float max, min;
                float delta;
                float hue = 0.0f;

                max = r; min = r;

                if (g > max) max = g;
                if (b > max) max = b;

                if (g < min) min = g;
                if (b < min) min = b;

                delta = max - min;

                if (r == max)
                {
                    hue = (g - b) / delta;
                }
                else if (g == max)
                {
                    hue = 2 + (b - r) / delta;
                }
                else if (b == max)
                {
                    hue = 4 + (r - g) / delta;
                }
                hue *= 60;

                if (hue < 0.0f)
                {
                    hue += 360.0f;
                }
                return hue;
            }
        }
        
        public float Saturation
        {
            get
            {
                float r = (float)R / 255.0f;
                float g = (float)G / 255.0f;
                float b = (float)B / 255.0f;

                float max, min;
                float l, s = 0;

                max = r; min = r;

                if (g > max) max = g;
                if (b > max) max = b;

                if (g < min) min = g;
                if (b < min) min = b;
                
                if (max != min)
                {
                    l = (max + min) / 2;

                    if (l <= .5)
                    {
                        s = (max - min) / (max + min);
                    }
                    else
                    {
                        s = (max - min) / (2 - max - min);
                    }
                }
                return s;
            }
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
