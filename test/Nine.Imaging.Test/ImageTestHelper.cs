namespace Nine.Imaging.Test
{
    using System;
    using System.IO;
    using Xunit;

    public static class ImageTestHelper
    {
        public static void VerifyAndSave(this Image image, string filename, Action<Stream> save = null)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var ms = new MemoryStream();
            if (save != null)
                save(ms);
            else
                image.SaveAsPng(ms);

            if (File.Exists(filename))
            {
                try
                {
                    Assert.Equal(ms.ToArray(), File.ReadAllBytes(filename));
                }
                catch
                {
                    File.WriteAllBytes(filename + ".actual", ms.ToArray());
                    throw;
                }
            }
            else
            {
                File.WriteAllBytes(filename, ms.ToArray());
            }
        }
    }
}