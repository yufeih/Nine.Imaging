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


            if (Parallelism > 1)
            {
                int partitionCount = Parallelism;

                Task[] tasks = new Task[partitionCount];

                for (int p = 0; p < partitionCount; p++)
                {
                    int current = p;
                    tasks[p] = Task.Run(() =>
                    {
                        int batchSize = rectangle.Height / partitionCount;
                        int yBegin = rectangle.Y + current * batchSize;
                        int yEnd = (current == partitionCount - 1 ? rectangle.Bottom : yBegin + batchSize);

                        Apply(target, source, rectangle, yBegin, yEnd);
                    });
                }

                Task.WaitAll(tasks);
            }
            else
            {
                for (var y = rectangle.Bottom; y < source.Height; y++)
                {
                    var i = y * source.Width;
                    var max = i + source.Width;

                    while (i < max)
                    {
                        target.Pixels[i] = source.Pixels[i];
                    }
                }


                Apply(target, source, rectangle, rectangle.Y, rectangle.Bottom);
            }

            return target;
        }

        protected abstract void Apply(Image target, Image source, Rectangle rectangle, int startY, int endY);
    }
}
