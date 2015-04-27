namespace Nine.Imaging
{
    using System;

    public class ImageBase
    {
        /// <summary>
        /// Returns all pixels of the image in g, b, r, a byte order.
        /// </summary>
        public byte[] Pixels => _pixels;
        private byte[] _pixels;
        
        public int PixelHeight => _pixelHeight;
        private int _pixelHeight;

        public int PixelWidth => _pixelWidth;
        private int _pixelWidth;

        public double PixelRatio => (double)PixelWidth / PixelHeight;

        public static int MaxWidth { get; set; } = int.MaxValue;
        public static int MaxHeight { get; set; } = int.MaxValue;
        
        public Color this[int x, int y]
        {
            get
            {
                int start = (y * _pixelWidth + x) * 4;

                return new Color(_pixels[start + 3], _pixels[start + 2], _pixels[start + 1], _pixels[start + 0]);
            }
            set
            {
                int start = (y * _pixelWidth + x) * 4;

                _pixels[start + 0] = value.B;
                _pixels[start + 1] = value.G;
                _pixels[start + 2] = value.R;
                _pixels[start + 3] = value.A;
            }
        }
        
        public Rectangle Bounds => new Rectangle(0, 0, _pixelWidth, _pixelHeight);

        public ImageBase() { }
        public ImageBase(int width, int height)
        {
            if (width < 0 || width > MaxWidth) throw new ArgumentOutOfRangeException($"Width must be between 0 and { MaxWidth }");
            if (height < 0 || height > MaxHeight) throw new ArgumentOutOfRangeException($"Height must be between 0 and { MaxHeight }");

            _pixelWidth = width;
            _pixelHeight = height;

            _pixels = new byte[_pixelWidth * _pixelHeight * 4];
        }

        public ImageBase(ImageBase other)
        {
            byte[] pixels = other.Pixels;

            _pixelWidth  = other._pixelWidth;
            _pixelHeight = other._pixelHeight;
            _pixels = new byte[pixels.Length];

            Array.Copy(pixels, _pixels, pixels.Length);
        }

        public void SetPixels(int width, int height, byte[] pixels)
        {
            if (width < 0 || width > MaxWidth) throw new ArgumentOutOfRangeException($"Width must be between 0 and { MaxWidth }");
            if (height < 0 || height > MaxHeight) throw new ArgumentOutOfRangeException($"Height must be between 0 and { MaxHeight }");
            if (pixels.Length != width * height * 4) throw new ArgumentException("Pixel array must have the length of width * height * 4.");

            _pixelWidth  = width;
            _pixelHeight = height;
            _pixels = pixels;
        }
    }
}
