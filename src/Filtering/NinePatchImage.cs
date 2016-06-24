namespace Nine.Imaging
{
    using System;
    using Nine.Imaging.Filtering;

    /// <summary>
    /// Represents a nine patch image from an android nine patch image format (http://developer.android.com/tools/help/draw9patch.html).
    /// </summary>
    public class NinePatchImage
    {
        private Image[] _patches;

        private readonly Image _image;

        public readonly int Left;
        public readonly int Right;
        public readonly int Top;
        public readonly int Bottom;

        public readonly int PaddingLeft;
        public readonly int PaddingRight;
        public readonly int PaddingTop;
        public readonly int PaddingBottom;

        public Image Image => _image;
        public Image[] Patches => _patches ?? (_patches = CreatePatches());

        private NinePatchImage(Image source, double scale, Image self)
        {
            _image = self;

            var maxX = source.Width - 1;
            var maxY = source.Height - 1;

            // Left/Right
            for (var x = 1; x <= maxX; x++) if (source[x, 0].A > 0) { Left = x - 1; break; }
            for (var x = 1; x <= maxX; x++) if (source[maxX - x, 0].A > 0) { Right = x - 1; break; }

            // Top/Bottom
            for (var y = 1; y <= maxY; y++) if (source[0, y].A > 0) { Top = y - 1; break; }
            for (var y = 1; y <= maxY; y++) if (source[0, maxY - y].A > 0) { Bottom = y - 1; break; }

            // PaddingLeft/PaddingRight
            for (var x = 1; x <= maxX; x++) if (source[x, maxY].A > 0) { PaddingLeft = x - 1; break; }
            for (var x = 1; x <= maxX; x++) if (source[maxX - x, maxY].A > 0) { PaddingRight = x - 1; break; }

            // PaddingTop/PaddingBottom
            for (var y = 1; y <= maxY; y++) if (source[maxX, y].A > 0) { PaddingTop = y - 1; break; }
            for (var y = 1; y <= maxY; y++) if (source[maxX, maxY - y].A > 0) { PaddingBottom = y - 1; break; }

            if (scale != 1)
            {
                Left = Math.Max(1, (int)Math.Round(Left * scale));
                Right = Math.Max(1, (int)Math.Round(Right * scale));
                Top = Math.Max(1, (int)Math.Round(Top * scale));
                Bottom = Math.Max(1, (int)Math.Round(Bottom * scale));

                PaddingLeft = Math.Max(1, (int)Math.Round(PaddingLeft * scale));
                PaddingRight = Math.Max(1, (int)Math.Round(PaddingRight * scale));
                PaddingTop = Math.Max(1, (int)Math.Round(PaddingTop * scale));
                PaddingBottom = Math.Max(1, (int)Math.Round(PaddingBottom * scale));
            }
        }

        public static NinePatchImage Create(Image source, double scale = 1)
        {
            if (source.Width < 3 || source.Height < 3) throw new ArgumentOutOfRangeException(nameof(source));

            var w = source.Width - 2;
            var h = source.Height - 2;
            var pixels = new byte[w * h * 4];

            for (var y = 0; y < h; y++)
            {
                Array.Copy(source.Pixels, ((y + 1) * source.Width + 1) * 4, pixels, y * w * 4, w * 4);
            }

            var self = new Image(w, h, pixels);

            if (scale != 1)
            {
                var resampler = new SuperSamplingSampler();
                var inner = new Image(w, h, pixels);

                w = Math.Max(3, (int)Math.Round((source.Width - 1) * scale));
                h = Math.Max(3, (int)Math.Round((source.Height - 1) * scale));

                self = resampler.Sample(inner, w, h);
            }

            return new NinePatchImage(source, scale, self);
        }

        private Image[] CreatePatches()
        {
            return new Image[9]
            {
                Patch(_image, 0, 0, Left, Top),
                Patch(_image, Left, 0, _image.Width - Left - Right, Top),
                Patch(_image, _image.Width - Right, 0, Right, Top),

                Patch(_image, 0, Top, Left, _image.Height - Top - Bottom),
                Patch(_image, Left, Top, _image.Width - Left - Right, _image.Height - Top - Bottom),
                Patch(_image, _image.Width - Right, Top, Right, _image.Height - Top - Bottom),

                Patch(_image, 0, _image.Height - Top, Left, Bottom),
                Patch(_image, Left, _image.Height - Top, _image.Width - Left - Right, Bottom),
                Patch(_image, _image.Width - Right, _image.Height - Top, Right, Bottom),
            };
        }

        private static Image Patch(Image image, int x, int y, int w, int h)
        {
            return ImageBaseOperations.Crop(image, new Rectangle(x, y, w, h));
        }
    }
}
