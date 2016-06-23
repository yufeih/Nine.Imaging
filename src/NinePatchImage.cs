namespace Nine.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Nine.Imaging.Filtering;

    /// <summary>
    /// Represents a nine patch image from an android nine patch image format (http://developer.android.com/tools/help/draw9patch.html).
    /// </summary>
    public class NinePatchImage : ImageBase
    {
        private readonly Lazy<IReadOnlyList<ImageBase>> patches;

        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Top { get; private set; }
        public int Bottom { get; private set; }

        public int PaddingLeft { get; private set; }
        public int PaddingRight { get; private set; }
        public int PaddingTop { get; private set; }
        public int PaddingBottom { get; private set; }

        public IReadOnlyList<ImageBase> Patches => patches.Value;

        public NinePatchImage(Stream stream, double scale = 1) : this(new Image(stream), scale)
        { }

        public NinePatchImage(ImageBase source, double scale = 1)
        {
            if (source.Width < 3) throw new ArgumentOutOfRangeException(nameof(Width));
            if (source.Height < 3) throw new ArgumentOutOfRangeException(nameof(Height));

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

            var w = source.Width - 2;
            var h = source.Height - 2;
            var pixels = new byte[w * h * 4];

            for (var y = 0; y < h; y++)
            {
                Array.Copy(source.Pixels, ((y + 1) * source.Width + 1) * 4, pixels, y * w * 4, w * 4);
            }

            SetPixels(w, h, pixels);

            if (scale != 1)
            {
                var resampler = new SuperSamplingSampler();
                var inner = new Image();
                inner.SetPixels(w, h, pixels);

                w = Math.Max(3, (int)Math.Round((source.Width - 1) * scale));
                h = Math.Max(3, (int)Math.Round((source.Height - 1) * scale));

                resampler.Sample(inner, this, w, h);
            }

            patches = new Lazy<IReadOnlyList<ImageBase>>(CreatePatches);
        }

        private IReadOnlyList<ImageBase> CreatePatches()
        {
            return new ImageBase[9]
            {
                Patch(0, 0, Left, Top),
                Patch(Left, 0, Width - Left - Right, Top),
                Patch(Width - Right, 0, Right, Top),

                Patch(0, Top, Left, Height - Top - Bottom),
                Patch(Left, Top, Width - Left - Right, Height - Top - Bottom),
                Patch(Width - Right, Top, Right, Height - Top - Bottom),

                Patch(0, Height - Top, Left, Bottom),
                Patch(Left, Height - Top, Width - Left - Right, Bottom),
                Patch(Width - Right, Height - Top, Right, Bottom),
            };
        }

        private ImageBase Patch(int x, int y, int w, int h)
        {
            var image = new Image();
            ImageBaseOperations.Crop(this, image, new Rectangle(x, y, w, h));
            return image;
        }
    }
}
