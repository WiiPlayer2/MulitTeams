using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTeams
{
    using System.Diagnostics;
    using System.IO;
    using System.Management;

    static class TeamsAPI
    {

        // Credit to https://techcommunity.microsoft.com/t5/microsoft-teams/multiple-instances-of-microsoft-teams-application/m-p/1306051
        public static async Task<Process> LaunchTeams(string profileName)
        {
            var teamsExeFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Teams");
            var updateExe = Path.Join(teamsExeFolder, "Update.exe");
            Environment.SetEnvironmentVariable("USERPROFILE", Path.Join(teamsExeFolder, "CustomProfiles", profileName));
            var updateProcess = Process.Start(updateExe, "--processStart \"Teams.exe\"");
            await updateProcess.WaitForExitAsync();
            var childProcesses = GetChildProcesses(updateProcess.Id);
            var windowProcess = childProcesses.First();
            return windowProcess;
        }

        public static IEnumerable<string> GetProfiles()
        {
            var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Teams", "CustomProfiles");
            return Directory.EnumerateDirectories(path).Select(Path.GetFileName).WhereNotNull();
        }

        private static List<Process> GetChildProcesses(int parentId)
        {
            var query = "Select * From Win32_Process Where ParentProcessId = "
                        + parentId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            var result = processList.Cast<ManagementObject>().Select(p =>
                Process.GetProcessById(Convert.ToInt32(p.GetPropertyValue("ProcessId")))
            ).ToList();

            return result;
        }
    }
}
