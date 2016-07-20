namespace Nine.Imaging
{
    using System;

    public partial class Image
    {
        public readonly int Width;
        public readonly int Height;

        /// <summary>
        /// Returns all pixels of the image in b, g, r, a byte order. Alpha is pre-multiplied.
        /// </summary>
        public readonly byte[] Pixels;

        public double AspectRatio => (double)Width / Height;

        public static int MaxWidth { get; set; } = int.MaxValue;
        public static int MaxHeight { get; set; } = int.MaxValue;

        public Color this[int x, int y]
        {
            get
            {
                int start = (y * Width + x) * 4;

                return new Color(
                    r: Pixels[start + 2],
                    g: Pixels[start + 1],
                    b: Pixels[start + 0],
                    a: Pixels[start + 3]);
            }
            set
            {
                int start = (y * Width + x) * 4;

                Pixels[start + 0] = value.B;
                Pixels[start + 1] = value.G;
                Pixels[start + 2] = value.R;
                Pixels[start + 3] = value.A;
            }
        }

        public Rectangle Bounds => new Rectangle(0, 0, Width, Height);

        public override string ToString() => $"Image {Width}x{Height}, {Width * Height * 4.0 / 1024}k";

        public Image(int width, int height)
        {
            if (width < 0 || width > MaxWidth) throw new ArgumentOutOfRangeException($"Width must be between 0 and { MaxWidth }");
            if (height < 0 || height > MaxHeight) throw new ArgumentOutOfRangeException($"Height must be between 0 and { MaxHeight }");

            Width = width;
            Height = height;
            Pixels = new byte[Width * Height * 4];
        }

        public Image(int width, int height, byte[] pixels)
        {
            if (width < 0 || width > MaxWidth) throw new ArgumentOutOfRangeException($"Width must be between 0 and { MaxWidth }");
            if (height < 0 || height > MaxHeight) throw new ArgumentOutOfRangeException($"Height must be between 0 and { MaxHeight }");
            if (pixels.Length != width * height * 4) throw new ArgumentOutOfRangeException($"Expected pixel array length { width * height * 4 }");

            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public static void PremultiplyPixels(byte[] pixels)
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var a = pixels[i + 3] / 255.0f;
                pixels[i] = (byte)(pixels[i] * a);
                pixels[i + 1] = (byte)(pixels[i + 1] * a);
                pixels[i + 2] = (byte)(pixels[i + 2] * a);
            }
        }

        public static void UnpremultiplyAlpha(byte[] pixels)
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var a = 255.0f / pixels[i + 3];
                pixels[i] = (byte)(pixels[i] * a);
                pixels[i + 1] = (byte)(pixels[i + 1] * a);
                pixels[i + 2] = (byte)(pixels[i + 2] * a);
            }
        }

        public virtual Image Clone()
        {
            var clonedPixels = new byte[Pixels.Length];

            Array.Copy(Pixels, clonedPixels, Pixels.Length);

            return new Image(Width, Height, clonedPixels);
        }
    }
}
