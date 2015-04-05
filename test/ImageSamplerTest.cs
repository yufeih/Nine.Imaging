namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using Nine.Imaging;
    using Nine.Imaging.Filtering;
    using System.Diagnostics;

    public class ImageSamplerTest
    {
        public static readonly TheoryData<string, int, int?, IImageSampler> Resizers = new TheoryData<string, int, int?, IImageSampler>();

        // TODO: Test alpha image
        static ImageSamplerTest()
        {
            Resizers.Add("Car.bmp", 200, null, new NearestNeighborSampler());
            Resizers.Add("Backdrop.jpg", 1000, null, new BilinearSampler());
            //Resizers.Add(new BicubicResizer());
            Resizers.Add("Car.bmp", 200, null, new SuperSamplingSampler());
        }

        [Theory]
        [MemberData("Resizers")]
        public void resize_image_using_sampler(string filename, int width, int? height, IImageSampler resizer)
        {
            if (!Directory.Exists("Resized")) Directory.CreateDirectory("Resized");

            var image = new Image(File.OpenRead($"TestImages/{ filename }"));
            var watch = Stopwatch.StartNew();

            if (height != null)
            {
                image = image.Resize(width, height.Value, resizer);
            }
            else
            {
                image = image.Resize(width, resizer);
            }

            Trace.WriteLine($"{ resizer.GetType().Name }: { watch.ElapsedMilliseconds}ms");

            height = height ?? width;
            var outputFile = $"Resized/{ Path.GetFileNameWithoutExtension(filename) }.{ resizer.GetType().Name }.{ width }x{ height }.jpg";
            using (var output = File.OpenWrite(outputFile))
            {
                image.SaveAsJpeg(output);
            }
        }
    }
}