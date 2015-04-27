namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using Nine.Imaging;
    using Nine.Imaging.Filtering;
    using System.Diagnostics;

    public class ImageSamplerTest
    {
        public static readonly TheoryData<string, int, int?, StretchMode, ParallelImageSampler> Resizers = new TheoryData<string, int, int?, StretchMode, ParallelImageSampler>();

        // TODO: Test alpha image
        static ImageSamplerTest()
        {
            Resizers.Add("Car.bmp", 1200, null, StretchMode.Fill, new NearestNeighborSampler());
            Resizers.Add("Backdrop.jpg", 1000, null, StretchMode.Fill, new BilinearSampler());
            Resizers.Add("Car.bmp", 1200, null, StretchMode.Fill, new BilinearSampler());
            Resizers.Add("Car.bmp", 200, null, StretchMode.Fill, new SuperSamplingSampler());
            Resizers.Add("Car.bmp", 1200, null, StretchMode.Fill, new SuperSamplingSampler());
        }

        [Theory]
        [MemberData("Resizers")]
        public void resize_image_using_sampler(string filename, int width, int? height, StretchMode mode, IImageSampler sampler)
        {
            var image = new Image(File.OpenRead($"TestImages/{ filename }"));
            var watch = Stopwatch.StartNew();

            if (height != null)
            {
                image = image.Resize(width, height.Value, mode, sampler);
            }
            else
            {
                image = image.Resize(width, mode, sampler);
            }

            Trace.WriteLine($"{ sampler.GetType().Name }: { watch.ElapsedMilliseconds}ms");

            height = height ?? width;

            
            var outputFile = $"Resized/{ Path.GetFileNameWithoutExtension(filename) }.{ mode }.{ sampler.GetType().Name }.{ width }x{ height }.jpg";
            image.VerifyAndSave(outputFile);
        }
    }
}