namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using Nine.Imaging;
    using Nine.Imaging.Filtering;
    using System.Diagnostics;

    public class ImageFilterTest
    {
        public static readonly TheoryData<string, IImageFilter> Filters = new TheoryData<string, IImageFilter>();

        static ImageFilterTest()
        {
            Filters.Add(typeof(Inverter).Name, new Inverter());
            Filters.Add(typeof(BlendingFilter).Name, new BlendingFilter(new Image(File.OpenRead("TestImages/Car.bmp"))) { Alpha = 0.25 });
            Filters.Add("GrayscaleBT709", new Grayscale { Coefficients = Grayscale.BT709 });
            Filters.Add("GrayscaleRMY", new Grayscale { Coefficients = Grayscale.RMY });
            Filters.Add(typeof(Sepia).Name, new Sepia());
            Filters.Add(typeof(Sobel).Name, new Sobel());
            Filters.Add(typeof(Prewitt).Name, new Prewitt());
            Filters.Add("Tint-yellow", new Tint { TintColor = new Color(255, 223, 119) });
            Filters.Add("Tint-yellow-hsb", new Tint { TintColor = new Color(255, 223, 119), UseHsbSpace = true });
            Filters.Add("Contrast-128", new Contrast(128));
            Filters.Add("Contrast--128", new Contrast(-128));
            Filters.Add("Brightness-128", new Brightness(128));
            Filters.Add("Brightness--128", new Brightness(-128));
            Filters.Add("GaussianBlur-2", new GaussianBlur { Variance = 2 });
            Filters.Add("GaussianBlur-5", new GaussianBlur { Variance = 5 });
            Filters.Add(nameof(CropCircle), new CropCircle());
        }

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