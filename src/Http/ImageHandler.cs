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
        private readonly IImageLoader[] loaders;
        private readonly IImageCache cache;
        private readonly string baseRoute;

        public ImageHandler(string baseRoute, params IImageLoader[] loaders) : this(baseRoute, new MemoryImageCache(), loaders, null) { }
        public ImageHandler(string baseRoute, IImageCache cache, params IImageLoader[] loaders) : this(baseRoute, cache, loaders, null) { }
        public ImageHandler(string baseRoute, IImageCache cache, IImageLoader[] loaders, Func<Type, string, object> convert = null, params Type[] declaredTypes)
        {
            if (loaders == null) throw new ArgumentNullException(nameof(loaders));

            baseRoute = baseRoute ?? "";
            if (baseRoute != "" && !baseRoute.EndsWith("/")) baseRoute += "/";

            this.baseRoute = "/" + baseRoute;
            this.cache = cache;
            this.loaders = loaders.ToArray();
            this.invoker = new ExtensionMethodInvoker(convert, new[] { typeof(ImageFiltering) }.Concat(declaredTypes).ToArray());

            ImageBase.MaxWidth = ImageBase.MaxHeight = 5000;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var id = request.RequestUri.AbsolutePath;
            if (!id.StartsWith(baseRoute, StringComparison.OrdinalIgnoreCase))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            id = id.Substring(baseRoute.Length);

            var cacheKey = request.RequestUri.PathAndQuery;
            var cached = await cache?.Get(cacheKey);
            if (cached != null)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(cached) };
            }

            var target = await LoadImage(id);
            if (target == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var query = request.RequestUri.Query;
            var result = string.IsNullOrEmpty(query) ? target : invoker.Invoke(target, query.Substring(1));
            
            var bytes = GetBytes(result);
            if (bytes != null)
            {
                await cache.Put(cacheKey, bytes);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(bytes) };
            }

            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        private byte[] GetBytes(object result)
        {
            var image = result as Image;
            if (image != null)
            {
                var ms = new MemoryStream();
                image.SaveAsPng(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }

            var stream = result as Stream;
            if (stream != null)
            {
                var ms = stream as MemoryStream;
                if (ms != null) return ms.ToArray();

                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return buffer;
            }

            var bytes = result as byte[];
            if (bytes != null)
            {
                return bytes;
            }

            return null;
        }

        private async Task<Image> LoadImage(string id)
        {
            foreach (var locator in loaders)
            {
                var image = await locator.LoadImage(id);
                if (image != null) return image;
            }

            return null;
        }
    }
}
