namespace Nine.Imaging.Filtering
{
    using System;
    using System.Threading.Tasks;

    public abstract class ParallelImageFilter : IImageFilter
    {
        /// <summary>
        /// Gets or sets the count of workers to run the filter in parallel.
        /// </summary>
        public int Parallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// This method is called before the filter is applied to prepare the filter.
        /// </summary>
        protected virtual void OnApply() { }

        public Image Apply(Image source, Rectangle rectangle)
        {
            var target = new Image(source.Width, source.Height);

            OnApply();

            var pitch = source.Width * 4;

            if (rectangle.Y > 0)
            {
                Array.Copy(source.Pixels, 0, target.Pixels, 0, rectangle.Y * pitch);
            }

            if (rectangle.Bottom < source.Height)
            {
                Array.Copy(source.Pixels, rectangle.Bottom * pitch, target.Pixels, rectangle.Bottom * pitch, (source.Height - rectangle.Bottom) * pitch);
            }

            if (rectangle.X > 0)
            {
                for (var y = rectangle.Y; y < rectangle.Bottom; y++)
                {
                    Array.Copy(source.Pixels, y * pitch, target.Pixels, y * pitch, rectangle.X * 4);
                }
            }

            if (rectangle.Right < source.Width)
            {
                for (var y = rectangle.Y; y < rectangle.Bottom; y++)
                {
                    Array.Copy(source.Pixels, y * pitch + rectangle.Right * 4, target.Pixels, y * pitch + rectangle.Right * 4, (source.Width - rectangle.Right) * 4);
                }
            }


            var partitionCount = Parallelism;
            if (partitionCount > 1)
            {
                Parallel.For(0, partitionCount, i =>
                {
                    int batchSize = rectangle.Height / partitionCount;
                    int yBegin = rectangle.Y + i * batchSize;
                    int yEnd = (i == partitionCount - 1 ? rectangle.Bottom : yBegin + batchSize);

                    Apply(target, source, rectangle, yBegin, yEnd);
                });
            }
            else
            {
                Apply(target, source, rectangle, rectangle.Y, rectangle.Bottom);
            }

            return target;
        }

        protected abstract void Apply(Image target, Image source, Rectangle rectangle, int startY, int endY);
    }
}
