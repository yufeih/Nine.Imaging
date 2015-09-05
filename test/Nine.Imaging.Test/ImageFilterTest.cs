namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using Nine.Imaging;
    using Nine.Imaging.Filtering;
    using System.Diagnostics;

    public class ImageFilterTest
    {
        public static readonly TheoryData<string, IImageFilter> Filters = new TheoryData<string, IImageFilter>
        {
            { typeof(Inverter).Name, new Inverter() },
            { typeof(BlendingFilter).Name, new BlendingFilter(new Image(File.OpenRead("TestImages/Car.bmp"))) { Alpha = 0.25 } },
            { "GrayscaleBT709", new Grayscale { Coefficients = Grayscale.BT709 } },
            { "GrayscaleRMY", new Grayscale { Coefficients = Grayscale.RMY } },
            { typeof(Sepia).Name, new Sepia() },
            { typeof(Sobel).Name, new Sobel() },
            { typeof(Prewitt).Name, new Prewitt() },
            { "Tint-yellow", new Tint { TintColor = new Color(255, 223, 119) } },
            { "Tint-yellow-hsb", new Tint { TintColor = new Color(255, 223, 119), UseHsbSpace = true } },
            { "Contrast-128", new Contrast(128) },
            { "Contrast--128", new Contrast(-128) },
            { "Brightness-128", new Brightness(128) },
            { "Brightness--128", new Brightness(-128) },
            { "GaussianBlur-2", new GaussianBlur { Variance = 2 } },
            { "GaussianBlur-5", new GaussianBlur { Variance = 5 } },
            { nameof(CropCircle), new CropCircle() },
        };

        [Theory]
        [MemberData("Filters")]
        public void filter_image(string name, IImageFilter filter)
        {
            var stream = File.OpenRead("TestImages/Backdrop.jpg");
            var image = new Image(stream);

            var watch = Stopwatch.StartNew();

            image = image.Filter(filter);

            Trace.WriteLine($"{ name }: { watch.ElapsedMilliseconds}ms");

            image.VerifyAndSave("TestResult/Filtered/" + name + ".png");
        }
    }
}