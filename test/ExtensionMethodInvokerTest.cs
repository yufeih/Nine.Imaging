namespace System
{
    using Xunit;
    using System.Text;

    public class ExtensionMethodInvokerTest
    {
        public readonly StringBuilder Text = new StringBuilder();

        private ExtensionMethodInvoker invoker = new ExtensionMethodInvoker(typeof(ExtensionMethodInvokerTestExtensions));

        [Theory]
        [InlineData("", "")]
        [InlineData("&&&", "")]
        [InlineData("int&none&int=1", "0none1")]
        [InlineData("Foo=10&height=20", "w10h20")]
        [InlineData("Foo=10&height=20&width=12", "w12h20")]
        [InlineData("str&str=ordinalIgnoreCase&str=1", "CurrentCultureOrdinalIgnoreCaseCurrentCultureIgnoreCase")]
        [InlineData("foo=10  &height\t  =20 &width= 12&\nBar&width=2&height= 4&bar=xyz", "w12h20xyz")]
        public void decode_then_encode_image_from_stream_should_succeed(string instruction, string expected)
        {
            Assert.Equal(this, invoker.Invoke(this, instruction));
            Assert.Equal(expected, Text.ToString());
        }
    }

    public static class ExtensionMethodInvokerTestExtensions
    {
        public static void Int(this ExtensionMethodInvokerTest self, int value)
        {
            self.Text.Append(value);
        }

        public static void None(this ExtensionMethodInvokerTest self)
        {
            self.Text.Append("none");
        }

        public static void Foo(this ExtensionMethodInvokerTest self, float width, double height)
        {
            self.Text.Append("w" + width);
            self.Text.Append("h" + height);
        }

        public static void Bar(this ExtensionMethodInvokerTest self, string text)
        {
            self.Text.Append(text);
        }

        public static void Str(this ExtensionMethodInvokerTest self, StringComparison cmp)
        {
            self.Text.Append(cmp.ToString());
        }
    }
}