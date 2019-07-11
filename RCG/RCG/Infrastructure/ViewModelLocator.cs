using RCG.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using RCG.Main.Models.SaveLoad;

namespace RCG.Infrastructure
{
    class ViewModelLocator
    {
        static MainViewModel mainViewModel;
        static HostTemplateViewModel hostTemplateViewModel;
        static HostViewModel hostViewModel;
        static PlayerViewModel playerViewModel;
        static SettingsViewModel settingsViewModel;
        static TutorialViewModel tutorialViewModel;

        public TutorialViewModel TutorialViewModel { get => tutorialViewModel ?? (tutorialViewModel = new TutorialViewModel()); }
        public SettingsViewModel SettingsViewModel { get => settingsViewModel ?? (settingsViewModel = new SettingsViewModel()); }
        public MainViewModel MainViewModel { get => mainViewModel ?? (mainViewModel = new MainViewModel(new JSONSaver<SaveObject>(), new JSONLoader<SaveObject>())); }
        public HostTemplateViewModel HostTemplateViewModel { get => hostTemplateViewModel ?? (hostTemplateViewModel = new HostTemplateViewModel()); }
        public HostViewModel HostViewModel { get => hostViewModel ?? (hostViewModel = new HostViewModel()); }
        public PlayerViewModel PlayerViewModel { get => playerViewModel ?? (playerViewModel = new PlayerViewModel()); }
    }
}
