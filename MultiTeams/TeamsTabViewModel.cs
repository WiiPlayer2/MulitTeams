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

    public class TeamsTabViewModel
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
                this.teamsProcess = await TeamsAPI.LaunchTeams(this.ProfileName);
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

        public void Disable()
        {
            User32.ShowWindow(this.teamsWindowHandle, ShowWindowCommand.SW_MINIMIZE);
        }
    }
}