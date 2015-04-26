namespace Nine.Imaging
{
    using System;

    /// <summary>
    /// Presents color in HSV color cylinder (http://en.wikipedia.org/wiki/HSL_and_HSV).
    /// </summary>
    public struct HsbColor
    {
        public float A;
        public float H;
        public float S;
        public float B;
        
        public static HsbColor FromRgb(Color rgb)
        {
            var hsb = new HsbColor();
            
            var r = (int)rgb.R;
            var g = (int)rgb.G;
            var b = (int)rgb.B;

            hsb.A = rgb.A / 255.0f;

            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;
            
            if (max == 0.0)
            {
                hsb.H = 0.0f;
                hsb.S = 0.0f;
                hsb.B = 0.0f;
                return hsb;
            }

            if (delta == 0.0) hsb.H = 0;
            else if (r == max) hsb.H = (g - b) / delta;
            else if (g == max) hsb.H = 2 + (b - r) / delta;
            else if (b == max) hsb.H = 4 + (r - g) / delta;
            hsb.H *= 60;
            if (hsb.H < 0.0) hsb.H += 360;
            
            hsb.S = delta / max;
            hsb.B = max / 255;
            
            return hsb;
        }

        public Color ToRgb()
        {
            if (this.S == 0.0)
            {
                return new Color(
                    (byte)(this.A * 255),
                    (byte)(this.B * 255),
                    (byte)(this.B * 255),
                    (byte)(this.B * 255));
            }
            
            var h = (this.H == 360) ? 0 : this.H / 60;
            var i = (int)(Math.Truncate(h));
            var f = h - i;

            var p = this.B * (1.0f - this.S);
            var q = this.B * (1.0f - (this.S * f));
            var t = this.B * (1.0f - (this.S * (1.0f - f)));

            float r, g, b;
            switch (i)
            {
                case 0:
                    r = this.B;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = this.B;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = this.B;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = this.B;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = this.B;
                    break;

                default:
                    r = this.B;
                    g = p;
                    b = q;
                    break;
            }

            return new Color(
                (byte)(Math.Round(this.A * 255)),
                (byte)(Math.Round(r * 255)),
                (byte)(Math.Round(g * 255)),
                (byte)(Math.Round(b * 255)));
        }

        public static bool operator ==(HsbColor a, HsbColor b)
        {
            return a.A == b.A && a.H == b.H && a.S == b.S && a.B == b.B;
        }

        public static bool operator !=(HsbColor a, HsbColor b)
        {
            return !(a.A == b.A && a.H == b.H && a.S == b.S && a.B == b.B);
        }

        public override int GetHashCode()
        {
            return H.GetHashCode() ^ S.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj is HsbColor) && this.Equals((HsbColor)obj));
        }

        public bool Equals(HsbColor other)
        {
            return A == other.A && H == other.H && S == other.S && B == other.B;
        }

        public override string ToString()
        {
            return $"Hue: { H }, Saturation: { S }, Brightness: { B }, Alpha: { A }";
        }
    }
}
