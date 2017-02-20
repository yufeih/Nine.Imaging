namespace Nine.Imaging
{
    using System;

    public struct CmykColor : IEquatable<CmykColor>
    {
        public float C;
        public float M;
        public float Y;
        public float K;

        public static CmykColor FromRgb(Color rgb)
        {
            float r = rgb.R / 255.0f;
            float g = rgb.G / 255.0f;
            float b = rgb.B / 255.0f;

            if (r == 0 && g == 0 && b == 0)
            {
                return new CmykColor { C = 0.0f, M = 0.0f, Y = 0.0f, K = 1.0f };
            }

            float c = 1.0f - r;
            float m = 1.0f - g;
            float y = 1.0f - b;

            float k = Math.Min(c, Math.Min(m, y));

            c = (c - k) / (1.0f - k);
            m = (m - k) / (1.0f - k);
            y = (y - k) / (1.0f - k);

            return new CmykColor { C = c, M = m, Y = y, K = k };
        }

        public Color ToRgb()
        {
            float key = 1.0f - K;
            float r = (1.0f - C) * key;
            float g = (1.0f - M) * key;
            float b = (1.0f - Y) * key;
            return new Color(r, g, b, 1.0f);
        }

        public static bool operator ==(CmykColor a, CmykColor b)
        {
            return a.C == b.C && a.M == b.M && a.Y == b.Y && a.K == b.K;
        }

        public static bool operator !=(CmykColor a, CmykColor b)
        {
            return !(a.C == b.C && a.M == b.M && a.Y == b.Y && a.K == b.K);
        }

        public override int GetHashCode()
        {
            return C.GetHashCode() ^ M.GetHashCode() ^ Y.GetHashCode() ^ K.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return ((obj is CmykColor) && this.Equals((CmykColor)obj));
        }

        public bool Equals(CmykColor other)
        {
            return C == other.C && M == other.M && Y == other.Y && K == other.K;
        }

        public override string ToString()
        {
            return $"C: { C }, M: { M }, Y: { Y }, K: { K }";
        }
    }
}
