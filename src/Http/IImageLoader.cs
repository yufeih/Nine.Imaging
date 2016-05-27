namespace Nine.Imaging.Http
{
    using System.Threading.Tasks;

    public interface IImageLoader
    {
        /// <summary>
        /// Loads the image with the specified id. Returns null if the image is not found.
        /// </summary>
        Task<Image> LoadImage(string id);
    }
}
