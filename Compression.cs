using System.IO;
using System.IO.Compression;

namespace toolbelt
{
    public static class Compression
    {
        public static MemoryStream InMemoryFromPath(string path, string searchPattern)
        {
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, searchPattern, new EnumerationOptions { RecurseSubdirectories = true }))
                    {
                        var archiveFile = archive.CreateEntry(file);
                        using (var writeStream = archiveFile.Open())
                        using (var readStream = File.OpenRead(file))
                        {
                            readStream.CopyTo(writeStream);
                        }
                    }
                }
            }
            return memoryStream;
        }
    }
}