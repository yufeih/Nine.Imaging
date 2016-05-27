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

        public void Apply(ImageBase target, ImageBase source, Rectangle rectangle)
        {
            OnApply();

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
                Apply(target, source, rectangle, rectangle.Y, rectangle.Bottom);
            }
        }

        protected abstract void Apply(ImageBase target, ImageBase source, Rectangle rectangle, int startY, int endY);
    }
}
