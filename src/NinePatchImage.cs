namespace Nine.Imaging
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents a nine patch image from an android nine patch image format (http://developer.android.com/tools/help/draw9patch.html).
    /// </summary>
    public class NinePatchImage : ImageBase
    {
        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Top { get; private set; }
        public int Bottom { get; private set; }

        public int PaddingLeft { get; private set; }
        public int PaddingRight { get; private set; }
        public int PaddingTop { get; private set; }
        public int PaddingBottom { get; private set; }

        public NinePatchImage(Stream stream) : this(new Image(stream))
        { }

        public NinePatchImage(ImageBase source)
        {
            var maxX = source.PixelWidth - 1;
            var maxY = source.PixelHeight - 1;

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

            var w = source.PixelWidth - 2;
            var h = source.PixelHeight - 2;
            var pixels = new byte[w * h * 4];

            for (var y = 0; y < h; y++)
            {
                Array.Copy(source.Pixels, ((y + 1) * source.PixelWidth + 1) * 4, pixels, y * w * 4, w * 4);
            }

            SetPixels(w, h, pixels);
        }
    }
}
