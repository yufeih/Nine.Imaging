// ===============================================================================
// Image_Operations.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;
using System.ComponentModel;

namespace Nine.Imaging.Filtering
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ImageFiltering
    {
        /// <summary>
        /// Applies the specified filter to the image.
        /// </summary>
        /// <param name="source">The image, where the filter should be applied to.</param>
        /// <param name="filters">The filter, which should be applied to.</param>
        /// <returns>
        /// A copy of the image with the applied filter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 	<para><paramref name="source"/> is null (Nothing in Visual Basic).</para>
        /// 	<para>- or -</para>
        /// 	<para><paramref name="filters"/> is null (Nothing in Visual Basic).</para>
        /// </exception>
        public static Image ApplyFilters(this Image source, params IImageFilter[] filters)
        {
            Rectangle bounds = source.Bounds;

            foreach (IImageFilter filter in filters)
            {
                source = PerformAction(source, true, (sourceImage, targetImage) => filter.Apply(targetImage, sourceImage, bounds));
            }

            return source;
        }

        /// <summary>
        /// Applies the specified filter to the image ath the specified rectangle.
        /// </summary>
        /// <param name="source">The image, where the filter should be applied to.</param>
        /// <param name="filters">The filter, which should be applied to.</param>
        /// <param name="rectangle">The rectangle, where the filter should be applied to.</param>
        /// <returns>
        /// A copy of the image with the applied filter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// 	<para><paramref name="source"/> is null (Nothing in Visual Basic).</para>
        /// 	<para>- or -</para>
        /// 	<para><paramref name="filters"/> is null (Nothing in Visual Basic).</para>
        /// </exception>
        public static Image ApplyFilters(this Image source, Rectangle rectangle, params IImageFilter[] filters)
        {
            foreach (IImageFilter filter in filters)
            {
                source = PerformAction(source, true, (sourceImage, targetImage) => filter.Apply(targetImage, sourceImage, rectangle));
            }

            return source;
        }

        public static Image GaussianBlur(this Image source, double amount) => source.ApplyFilters(new GaussianBlur { Variance = amount });
        public static Image Tint(this Image source, Color tint, bool hsb = false) => source.ApplyFilters(new Tint { TintColor = tint, UseHsbSpace = hsb });
        public static Image Grayscale(this Image source) => source.ApplyFilters(new Grayscale());
        public static Image Invert(this Image source) => source.ApplyFilters(new Inverter());
        public static Image Brightness(this Image source, int amount) => source.ApplyFilters(new Brightness(amount));
        public static Image Contrast(this Image source, int amount) => source.ApplyFilters(new Contrast(amount));
        public static Image CropCircle(this Image source, double radius = -1) => source.ApplyFilters(new CropCircle { Radius = radius });

        /// <summary>
        /// Cuts the image with the specified rectangle and returns a new image.
        /// </summary>
        /// <param name="source">The image, where a cutted copy should be made from.</param>
        /// <param name="bounds">The bounds of the new image.</param>
        /// <returns>The new cutted image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null
        /// (Nothing in Visual Basic).</exception>
        public static Image Crop(this Image source, Rectangle bounds)
        {
            return PerformAction(source, false, (sourceImage, targetImage) => ImageBaseOperations.Crop(sourceImage, targetImage, bounds));
        }

        /// <summary>
        /// Transforms the specified image by flipping and rotating it. First the image
        /// will be rotated, then the image will be flipped. A new image will be returned. The original image
        /// will not be changed.
        /// </summary>
        /// <param name="source">The image, which should be transformed.</param>
        /// <param name="rotationType">Type of the rotation.</param>
        /// <param name="flippingType">Type of the flipping.</param>
        /// <returns>The new and transformed image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null
        /// (Nothing in Visual Basic).</exception>
        public static Image Transform(this Image source, RotationType rotationType, FlippingType flippingType)
        {
            return PerformAction(source, false, (sourceImage, targetImage) => ImageBaseOperations.Transform(sourceImage, targetImage, rotationType, flippingType));
        }

        /// <summary>
        /// Resizes the specified image by using the specified <see cref="ParallelImageResampler"/> and
        /// return a new image with
        /// the spezified size which is a resized version of the passed image.
        /// </summary>
        /// <param name="source">The width of the new image. Must be greater than zero.</param>
        /// <param name="width">The width of the new image. Must be greater than zero.</param>
        /// <param name="height">The height of the new image. Must be greater than zero.</param>
        /// <returns>The new image.</returns>
        /// <exception cref="ArgumentNullException">
        /// 	<para><paramref name="resizer"/> is null (Nothing in Visual Basic).</para>
        /// 	<para>- or -</para>
        /// 	<para><paramref name="source"/> is null (Nothing in Visual Basic).</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 	<para><paramref name="width"/> is negative.</para>
        /// 	<para>- or -</para>
        /// 	<para><paramref name="height"/> is negative.</para>
        /// </exception>
        public static Image Resize(this Image source, int width, int height, IImageResampler resampler = null)
        {
            resampler = resampler ?? new SuperSamplingResampler();
            return PerformAction(source, false, (sourceImage, targetImage) => resampler.Sample(sourceImage, targetImage, width, height));
        }

        /// <summary>
        /// Resizes the specified image by using the specified <see cref="ParallelImageResampler"/> and
        /// returns new image which has the specified maximum
        /// extension in x and y direction.
        /// </summary>
        /// <param name="source">The source image to resize.</param>
        /// <param name="size">The maximum size of the image in x and y direction.</param>
        /// <returns>The resized image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="size"/> is negative.</exception>
        public static Image Resize(this Image source, int size, IImageResampler resampler = null)
        {
            int width = 0;
            int height = 0;

            float ratio = (float)source.PixelWidth / source.PixelHeight;

            if (source.PixelWidth > source.PixelHeight && ratio > 0)
            {
                width = size;
                height = (int)Math.Round(width / ratio);
            }
            else
            {
                height = size;
                width = (int)Math.Round(height * ratio);
            }

            resampler = resampler ?? new SuperSamplingResampler();
            return PerformAction(source, false, (sourceImage, targetImage) => resampler.Sample(sourceImage, targetImage, width, height));
        }

        private static Image PerformAction(Image source, bool clone, Action<ImageBase, ImageBase> action)
        {
            Image transformedImage = clone ? new Image(source) : new Image();
            
            action(source, transformedImage);

            foreach (ImageFrame frame in source.Frames)
            {
                ImageFrame temp = new ImageFrame();

                action(frame, temp);

                if (!clone)
                {
                    transformedImage.Frames.Add(temp);
                }
            }

            return transformedImage;
        }
    }
}
