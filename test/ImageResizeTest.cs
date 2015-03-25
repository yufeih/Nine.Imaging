namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using Nine.Imaging;
    using Nine.Imaging.Filtering;
    using System.Diagnostics;

    public class ImageResizeTest
    {
        public static readonly TheoryData<IImageResizer> Resizers = new TheoryData<IImageResizer>();

        static ImageResizeTest()
        {
            Resizers.Add(new NearestNeighborResizer());
            Resizers.Add(new BilinearResizer());
            //Resizers.Add(new BicubicResizer());
            //Resizers.Add(new SuperSamplingResizer());
        }

        [Theory]
        [MemberData("Resizers")]
        public void resize_image_using_resizer(IImageResizer resizer)
        {
            if (!Directory.Exists("Resized")) Directory.CreateDirectory("Resized");

            var image = new Image(File.OpenRead("TestImages/Car.bmp"));
            var watch = Stopwatch.StartNew();

            image = image.Resize(200, resizer);

            Trace.WriteLine($"{ resizer.GetType().Name }: { watch.ElapsedMilliseconds}ms");

            using (var output = File.OpenWrite($"Resized/" + resizer.GetType().Name + ".jpg"))
            {
                image.SaveAsJpeg(output);
            }
        }
    }
}