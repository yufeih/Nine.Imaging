namespace Nine.Imaging
{
    using System;

    public class ImageBase
    {
        private int _width;
        private int _height;
        private byte[] _pixels;

        public int Width => _width;
        public int Height => _height;

        /// <summary>
        /// Returns all pixels of the image in g, b, r, a byte order.
        /// </summary>
        public byte[] Pixels => _pixels;

        public double AspectRatio => (double)Width / Height;

        public static int MaxWidth { get; set; } = int.MaxValue;
        public static int MaxHeight { get; set; } = int.MaxValue;
        
        public Color this[int x, int y]
        {
            get
            {
                int start = (y * _width + x) * 4;

                return new Color(
                    r: _pixels[start + 2], 
                    g: _pixels[start + 1], 
                    b: _pixels[start + 0],
                    a: _pixels[start + 3]);
            }
            set
            {
                int start = (y * _width + x) * 4;

                _pixels[start + 0] = value.B;
                _pixels[start + 1] = value.G;
                _pixels[start + 2] = value.R;
                _pixels[start + 3] = value.A;
            }
        }
        
        public Rectangle Bounds => new Rectangle(0, 0, _width, _height);

        public override string ToString() => $"Image {Width}x{Height}, {Width * Height * 4.0 / 1024}k";

        public ImageBase() { }
        public ImageBase(int width, int height)
        {
            if (width < 0 || width > MaxWidth) throw new ArgumentOutOfRangeException($"Width must be between 0 and { MaxWidth }");
            if (height < 0 || height > MaxHeight) throw new ArgumentOutOfRangeException($"Height must be between 0 and { MaxHeight }");

            _width = width;
            _height = height;

            _pixels = new byte[_width * _height * 4];
        }

        public ImageBase(ImageBase other)
        {
            byte[] pixels = other.Pixels;

            _width  = other._width;
            _height = other._height;
            _pixels = new byte[pixels.Length];

            Array.Copy(pixels, _pixels, pixels.Length);
        }

        public void SetPixels(int width, int height, byte[] pixels)
        {
            if (width < 0 || width > MaxWidth) throw new ArgumentOutOfRangeException($"Width must be between 0 and { MaxWidth }");
            if (height < 0 || height > MaxHeight) throw new ArgumentOutOfRangeException($"Height must be between 0 and { MaxHeight }");
            if (pixels.Length != width * height * 4) throw new ArgumentException("Pixel array must have the length of width * height * 4.");

            _width  = width;
            _height = height;
            _pixels = pixels;
        }
    }
}
