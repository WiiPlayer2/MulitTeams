using System.Collections.Generic;
using System.Text;

namespace MultiTeams
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using EventBinder;

    public class MainViewModel
    {
        private TeamsTabViewModel? selectedTeamsTab;

        public ObservableCollection<TeamsTabViewModel> TeamsTabs { get; } = new ObservableCollection<TeamsTabViewModel>();

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
            //TeamsTabs = new List<TeamsTabViewModel>() { new(window, "bluehands"), new(window, "ROSEN"), };
        }

        public void OnLoaded(Window window)
        {
            foreach (var profile in TeamsAPI.GetProfiles())
            {
                TeamsTabs.Add(new (window, profile));
            }

            foreach (var tab in TeamsTabs)
            {
                tab.Enable();
                tab.Disable();
            }
        }
    }
}
