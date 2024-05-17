using System;
using System.IO;

namespace Backuper.SystemInfo
{
    internal sealed class ZeroKbTxtRemover
    {
        public static void RemoveZeroKbTxtFiles()
        {
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "MyTempFolder");

            if (!Directory.Exists(tempFolderPath))
            {
                Console.WriteLine("Target folder does not exist. No 0KB text files to remove.");
                return;
            }

            string[] txtFiles = Directory.GetFiles(tempFolderPath, "*.txt");
            foreach (string txtFile in txtFiles)
            {
                FileInfo fileInfo = new FileInfo(txtFile);
                if (fileInfo.Length == 0)
                {
                    
                    string text = File.ReadAllText(txtFile);

                    
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        
                        File.Delete(txtFile);
                        Console.WriteLine($"Removed 0KB text file: {txtFile}");
                    }
                }
            }
        }
    }
}
