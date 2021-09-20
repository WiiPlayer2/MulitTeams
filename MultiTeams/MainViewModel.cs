using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTeams
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;

    class AsyncActionCommand : ICommand
    {
        private readonly Func<object?, Task> execute;

        private readonly Func<object?, bool>? canExecute;

        public AsyncActionCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = default)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => this.canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object? parameter) => await this.execute(parameter);

        public event EventHandler? CanExecuteChanged;
    }

    class MainViewModel
    {
        private TeamsTabViewModel? selectedTeamsTab;

        public List<TeamsTabViewModel> TeamsTabs { get; }

        public TeamsTabViewModel? SelectedTeamsTab
        {
            get => this.selectedTeamsTab;
            set
            {
                if (ReferenceEquals(this.selectedTeamsTab, value))
                    return;

                this.selectedTeamsTab?.Disable();
                this.selectedTeamsTab = value;
                this.selectedTeamsTab?.Enable();
            }
        }

        public MainViewModel(Window window)
        {
            TeamsTabs = new List<TeamsTabViewModel>() { new(window, "bluehands"), new(window, "ROSEN"), };
        }
    }

    class TeamsTabViewModel
    {
        private readonly Window window;

        private Process? teamsProcess;

        private IntPtr teamsWindowHandle;

        public string ProfileName { get; }

        public TeamsTabViewModel(Window window, string profileName)
        {
            this.window = window;
            this.ProfileName = profileName;

            window.Closed += (sender, args) => this.teamsProcess?.Kill(true);
        }

        public async void Enable()
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            this.teamsProcess?.Refresh();
            if (this.teamsProcess?.HasExited ?? true)
            {
                teamsProcess = await LaunchTeams(ProfileName);
                await Task.Delay(5000);
            }

            if (this.teamsProcess.MainWindowHandle != IntPtr.Zero && this.teamsProcess.MainWindowHandle != this.teamsWindowHandle)
            {
                this.teamsWindowHandle = this.teamsProcess.MainWindowHandle;
                Natives.MoveWindow(this.teamsWindowHandle, 0, 100, 1600, 900, 1);
                Natives.SetParent(this.teamsWindowHandle, windowInteropHelper.Handle);
            }

            Natives.ShowWindow(this.teamsWindowHandle, 1);
        }

        private static async Task<Process> LaunchTeams(string profileName)
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
            Natives.ShowWindow(this.teamsWindowHandle, 0);
        }
    }
}
