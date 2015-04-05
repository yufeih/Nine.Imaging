namespace Nine.Imaging.Filtering
{
    using System;
    using System.Threading.Tasks;

    public abstract class ParallelImageResizer : IImageResizer
    {
        /// <summary>
        /// Gets or sets the count of workers to run the filter in parallel.
        /// </summary>
        public int Parallelism { get; set; } = Environment.ProcessorCount;

        public void Resize(ImageBase source, ImageBase target, int width, int height)
        {
            byte[] pixels = new byte[width * height * 4];

            if (Parallelism > 1)
            {
                int partitionCount = Parallelism;

                Task[] tasks = new Task[partitionCount];

                for (int p = 0; p < partitionCount; p++)
                {
                    int current = p;
                    tasks[p] = Task.Run(() =>
                    {
                        int batchSize = height / partitionCount;
                        int yBegin = current * batchSize;
                        int yEnd = (current == partitionCount - 1 ? height : yBegin + batchSize);

                        Resize(source, width, height, yBegin, yEnd, pixels);
                    });
                }

                Task.WaitAll(tasks);
            }
            else
            {
                Resize(source, width, height, 0, height, pixels);
            }

            target.SetPixels(width, height, pixels);
        }

        protected abstract void Resize(ImageBase source, int width, int height, int startY, int endY, byte[] pixels);
    }
}
