namespace Nine.Imaging.Filtering
{
    using System;

    struct HsbColor
    {
        public double A;
        public double H;
        public double S;
        public double B;

        public static HsbColor FromRgb(Color rgb)
        {
            var hsb = new HsbColor();
            
            var r = (int)rgb.R;
            var g = (int)rgb.G;
            var b = (int)rgb.B;
            var a = (int)rgb.A;
            
            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            double delta = max - min;
            
            if (max == 0.0)
            {
                hsb.H = 0.0;
                hsb.S = 0.0;
                hsb.B = 0.0;
                hsb.A = a;
                return hsb;
            }
            
            var alpha = (double)a;
            hsb.A = alpha / 255;
            
            if (r == max) hsb.H = (g - b) / delta;
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

            var p = this.B * (1.0 - this.S);
            var q = this.B * (1.0 - (this.S * f));
            var t = this.B * (1.0 - (this.S * (1.0 - f)));

            double r, g, b;
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
                (byte)(this.A * 255),
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255));
        }
    }
}
