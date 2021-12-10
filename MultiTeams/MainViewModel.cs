using System.Collections.Generic;
using System.Text;

namespace MultiTeams
{
    using System.ComponentModel;
    using System.Windows;
    using EventBinder;

    public class MainViewModel
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

        public void OnLoaded()
        {
            foreach (var tab in TeamsTabs)
            {
                tab.Enable();
                tab.Disable();
            }
        }
    }
}
