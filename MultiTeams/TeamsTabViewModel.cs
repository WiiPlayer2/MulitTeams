namespace MultiTeams
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;
    using Vanara.PInvoke;

    class TeamsTabViewModel
    {
        private readonly Window window;

        private Process? teamsProcess;

        private HWND teamsWindowHandle;

        public string ProfileName { get; }

        public TeamsTabViewModel(Window window, string profileName)
        {
            this.window = window;
            this.ProfileName = profileName;

            window.Closed += (sender, args) => this.teamsProcess?.Kill(true);
        }

        public async void Enable()
        {
            var windowInteropHelper = new WindowInteropHelper(this.window);
            this.teamsProcess?.Refresh();
            if (this.teamsProcess?.HasExited ?? true)
            {
                this.teamsProcess = await TeamsTabViewModel.LaunchTeams(this.ProfileName);
                await Task.Delay(5000);
            }

            if (this.teamsProcess.MainWindowHandle != IntPtr.Zero && this.teamsProcess.MainWindowHandle != this.teamsWindowHandle)
            {
                this.teamsWindowHandle = this.teamsProcess.MainWindowHandle;
                //User32.MoveWindow(this.teamsWindowHandle, 0, 100, 1600, 900, true);
                //User32.SetParent(this.teamsWindowHandle, windowInteropHelper.Handle);
            }

            await Task.Yield();
            User32.ShowWindow(this.teamsWindowHandle, ShowWindowCommand.SW_RESTORE);
        }

        // Credit to https://techcommunity.microsoft.com/t5/microsoft-teams/multiple-instances-of-microsoft-teams-application/m-p/1306051
        private static async Task<Process> LaunchTeams(string profileName)
        {
            var teamsExeFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Teams");
            var updateExe = Path.Join(teamsExeFolder, "Update.exe");
            Environment.SetEnvironmentVariable("USERPROFILE", Path.Join(teamsExeFolder, "CustomProfiles", profileName));
            var updateProcess = Process.Start(updateExe, "--processStart \"Teams.exe\"");
            await updateProcess.WaitForExitAsync();
            var childProcesses = TeamsTabViewModel.GetChildProcesses(updateProcess.Id);
            var windowProcess = childProcesses.First();
            return windowProcess;
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

        public void Disable()
        {
            User32.ShowWindow(this.teamsWindowHandle, ShowWindowCommand.SW_MINIMIZE);
        }
    }
}