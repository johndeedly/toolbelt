using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace toolbelt
{
    public static class ShellUtils
    {
        public static Task RunShellAsync(string fileName, string arguments, Stream outputStream = null, Stream errorStream = null)
        {
            return Task.Run(delegate
            {
                Process proc = new Process();
                proc.StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardError = true,
                    StandardErrorEncoding = new UTF8Encoding(false),
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = new UTF8Encoding(false)
                };
                proc.Start();
                Task stdout = proc.StandardOutput.BaseStream.CopyToAsync(outputStream ?? Console.OpenStandardOutput());
                Task stderr = proc.StandardError.BaseStream.CopyToAsync(errorStream ?? Console.OpenStandardError());
                proc.WaitForExit();
                Task.WaitAll(stdout, stderr);
            });
        }

        public static Task<string> RunShellTextAsync(string fileName, string arguments)
        {
            return Task.Run(delegate
            {
                using (MemoryStream mem = new MemoryStream())
                {
                    RunShellAsync(fileName, arguments, mem).Wait();
                    string txt = Encoding.UTF8.GetString(mem.ToArray());
                    return txt;
                }
            });
        }
    }
}