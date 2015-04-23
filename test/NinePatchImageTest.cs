namespace Nine.Imaging.Test
{
    using Xunit;
    using System.IO;
    using Nine.Imaging;

    public class NinePatchImageTest
    {
        [Theory]
        [InlineData("TestImages/NinePatch.png", 1)]
        [InlineData("TestImages/NinePatch.png", 0.5)]
        public void transform_images(string file, double scale)
        {
            if (!Directory.Exists("NinePatch")) Directory.CreateDirectory("NinePatch");

            var img = new NinePatchImage(File.OpenRead(file), scale);

            var name = 
                $"NinePatch/{ Path.GetFileNameWithoutExtension(file) }x{ scale } " +
                $"({ img.Left }, { img.Top }, { img.Right }, { img.Bottom })-" +
                $"({ img.PaddingLeft }, { img.PaddingTop }, { img.PaddingRight }, { img.PaddingBottom }).png";

            if (file == "TestImages/NinePatch.png" && scale == 1)
            {
                Assert.Equal(43, img.Left);
                Assert.Equal(53, img.Right);
                Assert.Equal(46, img.Top);
                Assert.Equal(44, img.Bottom);

                Assert.Equal(17, img.PaddingLeft);
                Assert.Equal(29, img.PaddingRight);
                Assert.Equal(21, img.PaddingTop);
                Assert.Equal(19, img.PaddingBottom);
            }

            using (var output = File.OpenWrite(name))
            {
                img.SaveAsPng(output);
            }

            for (int i = 0; i < img.Patches.Count; i++)
            {
                var patch = $"NinePatch/{ Path.GetFileNameWithoutExtension(file) }x{ scale }-{ i }.png";

                using (var output = File.OpenWrite(patch))
                {
                    img.Patches[i].SaveAsPng(output);
                }
            }
        }
    }
}