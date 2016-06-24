namespace Nine.Imaging.Filtering
{
    /// <summary>
    /// Interface for all image resizing algorithms.
    /// </summary>
    public interface IImageSampler
    {
        /// <summary>
        /// Resizes the specified source image by creating a new image with
        /// the specified size which is a resized version of the passed image..
        /// </summary>
        Image Sample(Image source, int width, int height);
    }
}
