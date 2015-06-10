namespace Nine.Imaging.Http
{
    using System.Threading.Tasks;

    public interface IImageCache
    {
        Task<byte[]> Get(string key);
        Task Put(string key, byte[] bytes);
    }
}
