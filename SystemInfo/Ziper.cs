using System;
using System.IO;
using System.IO.Compression;

namespace Backuper.SystemInfo
{
    internal sealed class Ziper
    {
        public static void ZipAndDeleteFolder(string folderPath)
        {
            try
            {
                string tempFolderPath = Path.Combine(Path.GetTempPath(), folderPath);
                string zipFilePath = Path.Combine(Path.GetTempPath(), "Backup.zip");

                ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);
                Directory.Delete(tempFolderPath, true);

                Console.WriteLine("Folder zipped and deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error zipping and deleting folder: {ex.Message}");
            }
        }
    }
}