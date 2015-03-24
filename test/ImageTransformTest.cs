namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Diagnostics;
    using Nine.Imaging.Filtering;
    using Nine.Imaging.IO;

    public class ImageTransformTest
    {
        public static readonly TheoryData<RotationType, FlippingType> Transforms = new TheoryData<RotationType, FlippingType>();

        static ImageTransformTest()
        {
            Transforms.Add(RotationType.None, FlippingType.Vertical);
            Transforms.Add(RotationType.None, FlippingType.Horizontal);

            Transforms.Add(RotationType.Rotate90, FlippingType.None);
            Transforms.Add(RotationType.Rotate180, FlippingType.None);
            Transforms.Add(RotationType.Rotate270, FlippingType.None);
        }

        [Theory]
        [MemberData("Transforms")]
        public void transform_images(RotationType rotate, FlippingType flip)
        {
            if (!Directory.Exists("Transformed")) Directory.CreateDirectory("Transformed");
            
            var image = new Image(File.OpenRead("TestImages/Backdrop.jpg"));
            var watch = Stopwatch.StartNew();

            image = image.Transform(rotate, flip);

            Trace.WriteLine($"{ rotate }-{ flip }: { watch.ElapsedMilliseconds}ms");

            var outputFilename = $"Transformed/{ rotate }-{ flip }.jpg";
            using (var output = File.OpenWrite(outputFilename))
            {
                image.SaveAsJpeg(output);
            }
        }
    }
}