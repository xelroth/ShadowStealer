using System.Diagnostics;
using System.IO;
using System.Management;

namespace Backuper.SystemInfo
{
    internal sealed class ProcessList
    {
        // Save process list
        public static void WriteProcesses()
        {
            string savePath = Path.Combine(Path.GetTempPath(), "MyTempFolder");
            int totalFileSize = 0;

            foreach (var process in Process.GetProcesses())
            {
                string processInfo = "NAME: " + process.ProcessName +
                                     "\n\tPID: " + process.Id +
                                     "\n\tEXE: " + ProcessExecutablePath(process) +
                                     "\n\n";
                totalFileSize += processInfo.Length;

                if (totalFileSize < 3000)
                {
                    File.AppendAllText(
                        Path.Combine(savePath, "Process.txt"),
                        processInfo
                    );
                }
                else
                {
                    break;
                }
            }
        }

        
        private static string ProcessExecutablePath(Process process)
        {
            try
            {
                if (process.MainModule != null) return process.MainModule.FileName;
            }
            catch
            {
                var query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                var searcher = new ManagementObjectSearcher(query);

                foreach (var o in searcher.Get())
                {
                    var item = (ManagementObject)o;
                    var id = item["ProcessID"];
                    var path = item["ExecutablePath"];

                    if (path != null && id.ToString() == process.Id.ToString()) return path.ToString();
                }
            }

            return "";
        }
    }
}
