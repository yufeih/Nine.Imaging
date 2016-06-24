namespace Nine.Imaging.Filtering
{
    public interface IImageFilter
    {
        Image Apply(Image target, Rectangle rectangle);
    }
}
