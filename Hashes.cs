using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace toolbelt
{
    public static class Hashes
    {
        public static string CalculateMD5FromStream(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(stream);
                return ByteArrayToHexString(hash);
            }
        }

        public static string CalculateSha1FromStream(Stream stream)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(stream);
                return ByteArrayToHexString(hash);
            }
        }

        public static string CalculateSha256FromStream(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                return ByteArrayToHexString(hash);
            }
        }

        private static string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}