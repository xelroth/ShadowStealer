using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Http;

namespace Backuper.payload.Components.Unity
{
    internal static class Comando
    {
        internal static async Task<bool> IsConnectionAvailable()
        {
            try
            {
                using (HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5.0) })
                {
                    HttpResponseMessage response = await httpClient.GetAsync("https://gstatic.com/generate_204");
                    return response.StatusCode ==
                           HttpStatusCode.NoContent;
                }
            }
            catch
            {
                return false;
            }
        }

        internal static string GeneratedRandString(int length)
        {
            Random random = new Random();
            string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < length; i++)
                result.Append(chars[random.Next(0, chars.Length)]);

            return result.ToString();
        }

        internal static bool Compress(string src, string dst)
        {
            if (Directory.Exists(src) && !File.Exists(dst) && Directory.Exists(Path.GetDirectoryName(dst)))
                try
                {
                    ZipFile.CreateFromDirectory(src, dst, CompressionLevel.Optimal, false, Encoding.UTF8);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }

            return false;
        }

        internal static void Copypaid(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src))
            {
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)));
            }

            foreach (var dir in Directory.GetDirectories(src))
            {
                Copypaid(dir, Path.Combine(dst, Path.GetFileName(dir)));
            }
        }

        internal static void CopyTree(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var file in Directory.GetFiles(src))
            {
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)));
            }

            foreach (var dir in Directory.GetDirectories(src))
            {
                CopyTree(dir, Path.Combine(dst, Path.GetFileName(dir)));
            }
        }
    }
}