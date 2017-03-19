namespace Nine.Imaging.Filtering
{
    using System;
    using System.Threading.Tasks;

    public abstract class ParallelImageSampler : IImageSampler
    {
        /// <summary>
        /// Gets or sets the count of workers to run the filter in parallel.
        /// </summary>
        public int Parallelism { get; set; } = Environment.ProcessorCount;

        public virtual Image Sample(Image source, int width, int height)
        {
            var pixels = new byte[width * height * 4];

            var partitionCount = Parallelism;
            if (partitionCount > 1)
            {
                Parallel.For(0, partitionCount, i =>
                {
                    int batchSize = height / partitionCount;
                    int yBegin = i * batchSize;
                    int yEnd = (i == partitionCount - 1 ? height : yBegin + batchSize);

                    Sample(source, width, height, yBegin, yEnd, pixels);
                });
            }
            else
            {
                Sample(source, width, height, 0, height, pixels);
            }

            return new Image(width, height, pixels);
        }

        protected abstract void Sample(Image source, int width, int height, int startY, int endY, byte[] pixels);
    }
}
