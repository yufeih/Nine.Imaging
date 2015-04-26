namespace Nine.Imaging.Http.Test
{
    using Xunit;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class ImageHandlerTest
    {
        [Theory]
        [InlineData("?width=100")]
        public async Task image_handler_rest_api_test(string parameter)
        {
            var client = new HttpClient(new ImageHandler()) { BaseAddress = new Uri("http://localhost/") };
            var response = await client.GetAsync("testimage" + parameter);
            var content = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync();
        }
    }
}