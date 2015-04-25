namespace Nine.Imaging.Test
{
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
    }
}