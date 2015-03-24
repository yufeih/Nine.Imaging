﻿namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Linq;
    using Nine.Imaging.Filtering;
    using Nine.Imaging.IO;
    using System.Diagnostics;

    public class ImageFilterTest
    {
        public static readonly TheoryData<string, IImageFilter> Filters = new TheoryData<string, IImageFilter>();

        static ImageFilterTest()
        {
            Filters.Add(typeof(Inverter).Name, new Inverter());
            Filters.Add(typeof(BlendingFilter).Name, new BlendingFilter(new Image(File.OpenRead("TestImages/Car.bmp"))) { GlobalAlphaFactor = 0.25 });
            Filters.Add(typeof(GrayscaleBT709).Name, new GrayscaleBT709());
            Filters.Add(typeof(GrayscaleRMY).Name, new GrayscaleRMY());
            Filters.Add(typeof(Sepia).Name, new Sepia());
            Filters.Add(typeof(SobelX).Name, new SobelX());
            Filters.Add(typeof(SobelY).Name, new SobelY());
            Filters.Add(typeof(PrewittX).Name, new PrewittX());
            Filters.Add(typeof(PrewittY).Name, new PrewittY());
            Filters.Add("Tint-yellow", new Tint { TintColor = new Color(255, 255, 0) });
            Filters.Add("Contrast-128", new Contrast(128));
            Filters.Add("Contrast--128", new Contrast(-128));
            Filters.Add("Brightness-128", new Brightness(128));
            Filters.Add("Brightness--128", new Brightness(-128));
            Filters.Add("GaussianBlur-2", new GaussianBlur { Variance = 2 });
            Filters.Add("GaussianBlur-5", new GaussianBlur { Variance = 5 });
        }

        [Theory]
        [MemberData("Filters")]
        public void decode_then_encode_image_from_stream_should_succeed(string name, IImageFilter filter)
        {
            if (!Directory.Exists("Filtered")) Directory.CreateDirectory("Filtered");

            var stream = File.OpenRead("TestImages/Backdrop.jpg");
            var image = new Image(stream);

            var watch = Stopwatch.StartNew();

            image = image.ApplyFilters(filter);

            Trace.WriteLine($"{ name }: { watch.ElapsedMilliseconds}ms");

            var outputFilename = "Filtered/" + name + ".jpg";
            using (var output = File.OpenWrite(outputFilename))
            {
                image.SaveAsJpeg(output);
            }
        }
    }
}