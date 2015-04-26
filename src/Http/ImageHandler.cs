namespace Nine.Imaging.Http
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Nine.Imaging.Filtering;

    public class ImageHandler : DelegatingHandler
    {
        private readonly ExtensionMethodInvoker invoker;

        public ImageHandler(Func<Type, string, object> convert = null, params Type[] declaredTypes)
        {
            this.invoker = new ExtensionMethodInvoker(convert, new[] { typeof(ImageFiltering) }.Concat(declaredTypes).ToArray());
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var target = new Image(64, 64);
            var result = invoker.Invoke(target, request.RequestUri.Query);
            var image = result as Image;
            if (image != null)
            {
                var ms = new MemoryStream();
                image.SaveAsPng(ms);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(ms) };
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
