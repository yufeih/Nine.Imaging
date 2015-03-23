// Copyright (c) 2008 Jeffrey Powers for Fluxcapacity Open Source.
// Under the MIT License, details: License.txt.

using System;

namespace FluxJpeg.Core
{
    struct ColorModel
    {
        public ColorSpace ColorSpace;
        public bool Opaque;
    }

    enum ColorSpace { Gray, YCbCr, RGB }

    class Image
    {
        private ColorModel _cm;
        private byte[][,] _raster;

        public byte[][,] Raster { get { return _raster; } }
        public ColorModel ColorModel { get { return _cm; } }

        /// <summary> X density (dots per inch).</summary>
        public double DensityX { get; set; }
        /// <summary> Y density (dots per inch).</summary>
        public double DensityY { get; set; }

        public int ComponentCount { get { return _raster.Length; } }

        /// <summary>
        /// Converts the colorspace of an image (in-place)
        /// </summary>
        /// <param name="cs">Colorspace to convert into</param>
        /// <returns>Self</returns>
        public Image ChangeColorSpace(ColorSpace cs)
        {
            // Colorspace is already correct
            if (_cm.ColorSpace == cs) return this;

            byte[] ycbcr = new byte[3];
            byte[] rgb = new byte[3];

            if (_cm.ColorSpace == ColorSpace.RGB && cs == ColorSpace.YCbCr)
            {
                /*
                 *  Y' =       + 0.299    * R'd + 0.587    * G'd + 0.114    * B'd
                    Cb = 128   - 0.168736 * R'd - 0.331264 * G'd + 0.5      * B'd
                    Cr = 128   + 0.5      * R'd - 0.418688 * G'd - 0.081312 * B'd
                 * 
                 */

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        YCbCr.fromRGB(ref _raster[0][x, y], ref _raster[1][x, y], ref _raster[2][x, y]);
                    }

                _cm.ColorSpace = ColorSpace.YCbCr;


            }
            else if (_cm.ColorSpace == ColorSpace.YCbCr && cs == ColorSpace.RGB)
            {

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        // 0 is LUMA
                        // 1 is BLUE
                        // 2 is RED

                        YCbCr.toRGB(ref _raster[0][x, y], ref _raster[1][x, y], ref _raster[2][x, y]);
                    }

                _cm.ColorSpace = ColorSpace.RGB;
            }
            else if (_cm.ColorSpace == ColorSpace.Gray && cs == ColorSpace.YCbCr)
            {
                // To convert to YCbCr, we just add two 128-filled chroma channels

                byte[,] Cb = new byte[width, height];
                byte[,] Cr = new byte[width, height];

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        Cb[x, y] = 128; Cr[x, y] = 128;
                    }

                _raster = new byte[][,] { _raster[0], Cb, Cr };

                _cm.ColorSpace = ColorSpace.YCbCr;
            }
            else if (_cm.ColorSpace == ColorSpace.Gray && cs == ColorSpace.RGB)
            {
                ChangeColorSpace(ColorSpace.YCbCr);
                ChangeColorSpace(ColorSpace.RGB);
            }
            else
            {
                throw new Exception("Colorspace conversion not supported.");
            }

            return this;
        }

        private int width; private int height;

        public int Width { get { return width; } }
        public int Height { get { return height; } }

        public Image(ColorModel cm, byte[][,] raster)
        {
            width = raster[0].GetLength(0);
            height = raster[0].GetLength(1);

            _cm = cm;
            _raster = raster;
        }

        public static byte[][,] CreateRaster(int width, int height, int bands)
        {
            // Create the raster
            byte[][,] raster = new byte[bands][,];
            for (int b = 0; b < bands; b++)
                raster[b] = new byte[width, height];
            return raster;
        }

        delegate void ConvertColor(ref byte c1, ref byte c2, ref byte c3);
    }
}
