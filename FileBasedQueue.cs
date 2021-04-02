using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace toolbelt
{
    public interface IAsyncQueue<T>
    {
        Task<int> Count(int timeout = 5000);
        Task Enqueue(T elem, int timeout = 5000);
        Task<T> Dequeue(int timeout = 5000);
        Task<IEnumerable<T>> GetEnumerable(int timeout = 5000);
        Task<T> Peek(int timeout = 5000);
    }

    public class FileBasedQueue<T> : IAsyncQueue<T> where T : class
    {
        string path;
        string lockPath;
        static Random random = new Random();
        static UTF8Encoding utf8 = new UTF8Encoding(false);
        static JsonSerializerOptions defaultOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true
        };

        public FileBasedQueue(string path)
        {
            if (!Directory.Exists(path))
                throw new FileNotFoundException();
            this.path = path;
            this.lockPath = $"{path}/lock";
        }

        public Task<int> Count(int timeout = 5000)
        {
            return Task.Run(delegate
            {
                IEnumerable<T> list = GetEnumerable(timeout).GetAwaiter().GetResult();
                return list.Count();
            });
        }

        public Task Enqueue(T elem, int timeout = 5000)
        {
            return Task.Run(delegate
            {
                if (elem == null)
                    throw new ArgumentNullException("elem");
                string json = JsonSerializer.Serialize(elem, defaultOptions);

                DateTimeOffset future = DateTimeOffset.Now.Add(TimeSpan.FromMilliseconds(timeout));
                string fileName = Path.GetRandomFileName();
                string filePath = $"{path}/{fileName}";

                while (DateTimeOffset.Now < future)
                {
                    if (File.Exists(filePath))
                    {
                        fileName = Path.GetRandomFileName();
                        filePath = $"{path}/{fileName}";
                        Task.Delay(random.Next(1, 10)).GetAwaiter().GetResult();
                        continue;
                    }

                    FileStream @lock;
                    if ((@lock = GetLock()) == null)
                    {
                        Task.Delay(random.Next(1, 10)).GetAwaiter().GetResult();
                        continue;
                    }

                    File.WriteAllText(filePath, json, utf8);

                    ReleaseLock(ref @lock);
                    return;
                }

                throw new TimeoutException();
            });
        }

        public Task<T> Dequeue(int timeout = 5000)
        {
            return Task.Run(delegate
            {
                DateTimeOffset future = DateTimeOffset.Now.Add(TimeSpan.FromMilliseconds(timeout));
                while (DateTimeOffset.Now < future)
                {
                    FileStream @lock;
                    if ((@lock = GetLock()) == null)
                    {
                        Task.Delay(random.Next(1, 10)).GetAwaiter().GetResult();
                        continue;
                    }

                    foreach (var filePath in Directory.EnumerateFiles(path))
                    {
                        if (Path.GetFileName(filePath) == "lock")
                            continue;
                        try
                        {
                            string json = File.ReadAllText(filePath, utf8);
                            T elem = (T)JsonSerializer.Deserialize(json, typeof(T), defaultOptions);

                            File.Delete(filePath);
                            ReleaseLock(ref @lock);
                            return elem;
                        }
                        catch (IOException)
                        { }
                    }

                    ReleaseLock(ref @lock);
                    return null;
                }

                throw new TimeoutException();
            });
        }

        public Task<IEnumerable<T>> GetEnumerable(int timeout = 5000)
        {
            return Task.Run(delegate
            {
                DateTimeOffset future = DateTimeOffset.Now.Add(TimeSpan.FromMilliseconds(timeout));
                while (DateTimeOffset.Now < future)
                {
                    FileStream @lock;
                    if ((@lock = GetLock()) == null)
                    {
                        Task.Delay(random.Next(1, 10)).GetAwaiter().GetResult();
                        continue;
                    }

                    List<T> list = new List<T>();
                    foreach (var filePath in Directory.EnumerateFiles(path))
                    {
                        if (Path.GetFileName(filePath) == "lock")
                            continue;
                        try
                        {
                            string json = File.ReadAllText(filePath, utf8);
                            T elem = (T)JsonSerializer.Deserialize(json, typeof(T), defaultOptions);

                            list.Add(elem);
                        }
                        catch (IOException)
                        { }
                    }

                    ReleaseLock(ref @lock);
                    return (IEnumerable<T>)list;
                }

                throw new TimeoutException();
            });
        }

        public Task<T> Peek(int timeout = 5000)
        {
            return Task.Run(delegate
            {
                DateTimeOffset future = DateTimeOffset.Now.Add(TimeSpan.FromMilliseconds(timeout));
                while (DateTimeOffset.Now < future)
                {
                    FileStream @lock;
                    if ((@lock = GetLock()) == null)
                    {
                        Task.Delay(random.Next(1, 10)).GetAwaiter().GetResult();
                        continue;
                    }

                    foreach (var filePath in Directory.EnumerateFiles(path))
                    {
                        if (Path.GetFileName(filePath) == "lock")
                            continue;
                        try
                        {
                            string json = File.ReadAllText(filePath, utf8);
                            T elem = (T)JsonSerializer.Deserialize(json, typeof(T), defaultOptions);

                            ReleaseLock(ref @lock);
                            return elem;
                        }
                        catch (IOException)
                        { }
                    }

                    ReleaseLock(ref @lock);
                    return null;
                }

                throw new TimeoutException();
            });
        }

        private FileStream GetLock()
        {
            try
            {
                // sync
                var @lock = new FileStream(lockPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                @lock.Write(utf8.GetBytes(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()));
                @lock.Flush();
                return @lock;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ReleaseLock(ref FileStream @lock)
        {
            @lock.Dispose();
            @lock = null;
            File.Delete(lockPath);
        }
    }
}