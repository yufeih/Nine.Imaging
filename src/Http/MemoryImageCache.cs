namespace Nine.Imaging.Http
{
    using System;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    public class MemoryImageCache : IImageCache
    {
        private readonly CacheItemPolicy policy;
        private readonly MemoryCache cache = new MemoryCache(nameof(MemoryImageCache));

        public MemoryImageCache() { }
        public MemoryImageCache(TimeSpan slidingExpiration)
        {
            this.policy = new CacheItemPolicy { SlidingExpiration = slidingExpiration };
        }

        public Task<byte[]> Get(string key)
        {
            return Task.FromResult((byte[])cache.Get(key));
        }

        public Task Put(string key, byte[] bytes)
        {
            cache.Set(key, bytes, policy);
            return Task.FromResult(0);
        }
    }
}
