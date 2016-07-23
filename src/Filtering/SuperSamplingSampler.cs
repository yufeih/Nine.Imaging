namespace Nine.Imaging.Filtering
{
    using System;

    public class SuperSamplingSampler : ParallelImageSampler
    {
        private static readonly BilinearSampler bilinear = new BilinearSampler();

        public override Image Sample(Image source, int width, int height)
        {
            if (width < source.Width && height < source.Height)
            {
                // Use super sampling only in case of minification
                return base.Sample(source, width, height);
            }
            else
            {
                return bilinear.Sample(source, width, height);
            }
        }

        protected override void Sample(Image source, int width, int height, int startY, int endY, byte[] pixels)
        {
            byte[] sourcePixels = source.Pixels;

            int sourceWidth = source.Width, offset;

            double factorX = (double)source.Width / width;
            double factorY = (double)source.Height / height;

            int b, t, l, r;
            double wb, wt, wl, wr, weight, a;

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

                        a = sourcePixels[offset + 3];

                        sr += sourcePixels[offset + 0] * wl * a;
                        sg += sourcePixels[offset + 1] * wl * a;
                        sb += sourcePixels[offset + 2] * wl * a;
                        sa += a * wl;

                        // Right
                        offset = (srcY * sourceWidth + r) << 2;

                        a = sourcePixels[offset + 3];

                        sr += sourcePixels[offset + 0] * wr * a;
                        sg += sourcePixels[offset + 1] * wr * a;
                        sb += sourcePixels[offset + 2] * wr * a;
                        sa += a * wr;
                    }

                    totalWeight += (b - t - 1) * (wr + wl);

                    for (int srcX = l + 1; srcX < r; ++srcX)
                    {
                        // Top
                        offset = (t * sourceWidth + srcX) << 2;

                        a = sourcePixels[offset + 3];

                        sr += sourcePixels[offset + 0] * wt * a;
                        sg += sourcePixels[offset + 1] * wt * a;
                        sb += sourcePixels[offset + 2] * wt * a;
                        sa += a * wt;

                        // Bottom
                        offset = (b * sourceWidth + srcX) << 2;

                        a = sourcePixels[offset + 3];

                        sr += sourcePixels[offset + 0] * wb * a;
                        sg += sourcePixels[offset + 1] * wb * a;
                        sb += sourcePixels[offset + 2] * wb * a;
                        sa += a * wb;
                    }

                    totalWeight += (r - l - 1) * (wt + wb);

                    // Center
                    for (int srcY = t + 1; srcY < b; ++srcY)
                    {
                        for (int srcX = l + 1; srcX < r; ++srcX)
                        {
                            offset = (srcY * sourceWidth + srcX) << 2;

                            a = sourcePixels[offset + 3];

                            sr += sourcePixels[offset + 0] * a;
                            sg += sourcePixels[offset + 1] * a;
                            sb += sourcePixels[offset + 2] * a;
                            sa += a;
                        }
                    }

                    totalWeight += (r - l - 1) * (b - t - 1);

                    // Corner
                    offset = (t * sourceWidth + l) << 2;
                    totalWeight += weight = wt * wl;

                    a = sourcePixels[offset + 3];

                    sr += sourcePixels[offset + 0] * weight * a;
                    sg += sourcePixels[offset + 1] * weight * a;
                    sb += sourcePixels[offset + 2] * weight * a;
                    sa += a * weight;

                    offset = (t * sourceWidth + r) << 2;
                    totalWeight += weight = wt * wr;

                    a = sourcePixels[offset + 3];

                    sr += sourcePixels[offset + 0] * weight * a;
                    sg += sourcePixels[offset + 1] * weight * a;
                    sb += sourcePixels[offset + 2] * weight * a;
                    sa += a * weight;

                    offset = (b * sourceWidth + l) << 2;
                    totalWeight += weight = wb * wl;

                    a = sourcePixels[offset + 3];

                    sr += sourcePixels[offset + 0] * weight * a;
                    sg += sourcePixels[offset + 1] * weight * a;
                    sb += sourcePixels[offset + 2] * weight * a;
                    sa += a * weight;

                    offset = (b * sourceWidth + r) << 2;
                    totalWeight += weight = wb * wr;

                    a = sourcePixels[offset + 3];

                    sr += sourcePixels[offset + 0] * weight * a;
                    sg += sourcePixels[offset + 1] * weight * a;
                    sb += sourcePixels[offset + 2] * weight * a;
                    sa += a * weight;

                    offset = (y * width + x) << 2;
                    weight = 1.0 / totalWeight;

                    a = sa * weight;

                    pixels[offset + 0] = (byte)(sr * weight / a);
                    pixels[offset + 1] = (byte)(sg * weight / a);
                    pixels[offset + 2] = (byte)(sb * weight / a);
                    pixels[offset + 3] = (byte)a;
                }
            }
        }
    }
}
