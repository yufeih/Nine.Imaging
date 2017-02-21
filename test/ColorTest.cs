namespace Nine.Imaging.Test
{
    using System;
    using Xunit;

    public class ColorTest
    {
        [Theory]
        [InlineData("1, 2, 3", 255, 1, 2, 3)]
        [InlineData("123, 2, 3, 4", 123, 2, 3, 4)]
        [InlineData("#CcC", 255, 204, 204, 204)]
        [InlineData("#fFA67C", 255, 255, 166, 124)]
        [InlineData("#FF000000", 255, 0, 0, 0)]
        [InlineData("CCC", 255, 204, 204, 204)]
        [InlineData("FFa67C", 255, 255, 166, 124)]
        [InlineData("FF000000", 255, 0, 0, 0)]
        public void parse_color(string text, int a, int r, int g, int b)
        {
            var c = Color.Parse(text);
            Assert.Equal(a, c.A);
            Assert.Equal(r, c.R);
            Assert.Equal(g, c.G);
            Assert.Equal(b, c.B);
        }

        [Fact]
        public void convert_between_cmyk_and_rgb()
        {
            foreach (var c in new[] { Color.White, Color.Black, Color.Red, Color.Green, Color.Black, new Color(123, 123, 123) })
            {
                Assert.Equal(c, Color.FromCmyk(c.ToCmyk()));
            }

            // CMYK has data loss
            //var random = new Random(0);
            //for (var i = 0; i < 1000; i++)
            //{
            //    var c = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
            //    var cmyk = c.ToCmyk();
            //    Assert.Equal(c, Color.FromCmyk(cmyk.C, cmyk.M, cmyk.Y, cmyk.K));
            //}
        }

        [Fact]
        public void convert_between_hsb_and_rgb()
        {
            foreach (var c in new[] { Color.Transparent, Color.White, Color.Black, Color.Red, Color.Green, Color.Black, new Color(123, 123, 123) })
            {
                Assert.Equal(c, Color.FromHsb(c.ToHsb()));
            }

            var random = new Random(0);
            for (var i = 0; i < 1000; i++)
            {
                var c = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
                var hsb = c.ToHsb();
                Assert.Equal(c, Color.FromHsb(hsb.H, hsb.S, hsb.B));
            }
        }

        [Fact]
        public void hsb_color_is_using_hsv_cylinder()
        {
            // http://stackoverflow.com/questions/4106363/converting-rgb-to-hsb-colors
            // http://en.wikipedia.org/wiki/HSL_and_HSV
            Assert.Equal(Color.Red, Color.FromHsb(0, 1, 1));
        }
    }
}