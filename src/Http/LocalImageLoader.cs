namespace Nine.Imaging.Http
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class LocalImageLoader : IImageLoader
    {
        private readonly string prefix;
        private readonly string baseDirectory;

        public LocalImageLoader(string prefix = "local", string baseDirectory = null)
        {
            prefix = prefix ?? "";
            if (prefix != "" && !prefix.EndsWith("/")) prefix += "/";

            if (string.IsNullOrEmpty(baseDirectory))
                baseDirectory = Environment.CurrentDirectory;
            else if (!Path.IsPathRooted(baseDirectory))
                baseDirectory = Path.Combine(Environment.CurrentDirectory, baseDirectory);

            this.prefix = prefix;
            this.baseDirectory = baseDirectory;
        }

        public Task<Image> LoadImage(string id)
        {
            if (!id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return Task.FromResult<Image>(null);

            var path = id.Substring(prefix.Length);
            path = Path.Combine(baseDirectory, path);

            if (!File.Exists(path)) return Task.FromResult<Image>(null);

            using (var stream = File.OpenRead(path))
            {
                return Task.FromResult(Image.Load(stream));
            }
        }
    }
}
