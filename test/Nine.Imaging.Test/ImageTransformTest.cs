namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using System.Diagnostics;
    using Nine.Imaging.Filtering;
    using Nine.Imaging;

    public class ImageTransformTest
    {
        public static readonly TheoryData<RotationType, FlippingType> Transforms = new TheoryData<RotationType, FlippingType>
        {
            { RotationType.None, FlippingType.Vertical },
            { RotationType.None, FlippingType.Horizontal },

            { RotationType.Rotate90, FlippingType.None },
            { RotationType.Rotate180, FlippingType.None },
            { RotationType.Rotate270, FlippingType.None },
        };

        [Theory]
        [MemberData("Transforms")]
        public void transform_images(RotationType rotate, FlippingType flip)
        {
            var image = new Image(File.OpenRead("TestImages/Backdrop.jpg"));
            var watch = Stopwatch.StartNew();

            image = image.Transform(rotate, flip);

            Trace.WriteLine($"{ rotate }-{ flip }: { watch.ElapsedMilliseconds}ms");

            image.VerifyAndSave($"TestResult/Transformed/{ rotate }-{ flip }.jpg");
        }
    }
}