namespace Nine.Imaging.Filtering
{
    using System;

    public class SuperSamplingSampler : ParallelImageSampler
    {
        private readonly BilinearSampler bilinear = new BilinearSampler();

        public override void Sample(ImageBase source, ImageBase target, int width, int height)
        {
            if (width < source.Width && height < source.Height)
            {
                // Use super sampling only in case of minification
                base.Sample(source, target, width, height);
            }
            else
            {
                bilinear.Sample(source, target, width, height);
            }
        }

        protected override void Sample(ImageBase source, int width, int height, int startY, int endY, byte[] pixels)
        {
            byte[] sourcePixels = source.Pixels;

            int sourceWidth = source.Width, offset;

            double factorX = (double)source.Width / width;
            double factorY = (double)source.Height / height;

            int b, t, l, r;
            double wb, wt, wl, wr, weight;

            for (int y = startY; y < endY; y++)
            {
                wt = y * factorY;
                t = (int)Math.Floor(wt);
                wt = 1 - (wt - t);

                wb = (y + 1) * factorY;
                b = (int)Math.Floor(wb - 0.000001);
                wb = wb - b;

                for (int x = 0; x < width; x++)
                {
                    wl = x * factorX;
                    l = (int)Math.Floor(wl);
                    wl = 1 - (wl - l);

                    wr = (x + 1) * factorX;
                    r = (int)Math.Floor(wr - 0.000001);
                    wr = wr - r;

                    double sr = 0, sg = 0, sb = 0, sa = 0;
                    double totalWeight = 0;

                    for (int srcY = t + 1; srcY < b; ++srcY)
                    {
                        // Left
                        offset = (srcY * sourceWidth + l) << 2;

                        sr += sourcePixels[offset + 0] * wl;
                        sg += sourcePixels[offset + 1] * wl;
                        sb += sourcePixels[offset + 2] * wl;
                        sa += sourcePixels[offset + 3] * wl;

                        // Right
                        offset = (srcY * sourceWidth + r) << 2;

                        sr += sourcePixels[offset + 0] * wr;
                        sg += sourcePixels[offset + 1] * wr;
                        sb += sourcePixels[offset + 2] * wr;
                        sa += sourcePixels[offset + 3] * wr;
                    }

                    totalWeight += (b - t - 1) * (wr + wl);

                    for (int srcX = l + 1; srcX < r; ++srcX)
                    {
                        // Top
                        offset = (t * sourceWidth + srcX) << 2;

                        sr += sourcePixels[offset + 0] * wt;
                        sg += sourcePixels[offset + 1] * wt;
                        sb += sourcePixels[offset + 2] * wt;
                        sa += sourcePixels[offset + 3] * wt;

                        // Bottom
                        offset = (b * sourceWidth + srcX) << 2;

                        sr += sourcePixels[offset + 0] * wb;
                        sg += sourcePixels[offset + 1] * wb;
                        sb += sourcePixels[offset + 2] * wb;
                        sa += sourcePixels[offset + 3] * wb;
                    }

                    totalWeight += (r - l - 1) * (wt + wb);

                    // Center
                    for (int srcY = t + 1; srcY < b; ++srcY)
                    {
                        for (int srcX = l + 1; srcX < r; ++srcX)
                        {
                            offset = (srcY * sourceWidth + srcX) << 2;

                            sr += sourcePixels[offset + 0];
                            sg += sourcePixels[offset + 1];
                            sb += sourcePixels[offset + 2];
                            sa += sourcePixels[offset + 3];
                        }
                    }

                    totalWeight += (r - l - 1) * (b - t - 1);

                    // Corner
                    offset = (t * sourceWidth + l) << 2;
                    totalWeight += weight = wt * wl;
                    sr += sourcePixels[offset + 0] * weight;
                    sg += sourcePixels[offset + 1] * weight;
                    sb += sourcePixels[offset + 2] * weight;
                    sa += sourcePixels[offset + 3] * weight;

                    offset = (t * sourceWidth + r) << 2;
                    totalWeight += weight = wt * wr;
                    sr += sourcePixels[offset + 0] * weight;
                    sg += sourcePixels[offset + 1] * weight;
                    sb += sourcePixels[offset + 2] * weight;
                    sa += sourcePixels[offset + 3] * weight;

                    offset = (b * sourceWidth + l) << 2;
                    totalWeight += weight = wb * wl;
                    sr += sourcePixels[offset + 0] * weight;
                    sg += sourcePixels[offset + 1] * weight;
                    sb += sourcePixels[offset + 2] * weight;
                    sa += sourcePixels[offset + 3] * weight;

                    offset = (b * sourceWidth + r) << 2;
                    totalWeight += weight = wb * wr;
                    sr += sourcePixels[offset + 0] * weight;
                    sg += sourcePixels[offset + 1] * weight;
                    sb += sourcePixels[offset + 2] * weight;
                    sa += sourcePixels[offset + 3] * weight;

                    offset = (y * width + x) << 2;
                    weight = 1.0 / totalWeight;

                    pixels[offset + 0] = (byte)(sr * weight);
                    pixels[offset + 1] = (byte)(sg * weight);
                    pixels[offset + 2] = (byte)(sb * weight);
                    pixels[offset + 3] = (byte)(sa * weight);
                }
            }
        }
    }
}
