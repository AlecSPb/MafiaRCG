using Acr.UserDialogs;
using RCG.Infrastructure;
using RCG.Main.Infrastructure;
using RCG.Main.Models;
using RCG.Main.Models.Enums;
using RCG.Main.Models.SaveLoad;
using RCG.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace RCG.ViewModels
{
    class MainViewModel : IBack
    {
        IFileSaver<SaveObject> saver;
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RCG" , "RCGData.json");
        ICommand hostCommand; 
        ICommand playerCommand;
        ICommand tutorialCommand;
        ICommand exitCommand;
        ICommand settingsCommand;

        public ImageSource BackgroundImage { get; } = ImageSource.FromResource("RCG.Main.Resources.Pictures.Background.jpg", typeof(Host).Assembly);
        public LocalizationManager Lm => LocalizationManager.GetLocalizationManager();
        public ICommand HostCommand { get => hostCommand ?? (hostCommand = new RelayCommand(Host)); }
        public ICommand PlayerCommand { get => playerCommand ?? (playerCommand = new RelayCommand(Player)); }
        public ICommand TutorialCommand { get => tutorialCommand ?? (tutorialCommand = new RelayCommand(Tutorial)); }
        public ICommand ExitCommand { get => exitCommand ?? (exitCommand = new RelayCommand(Exit)); }
        public ICommand SettingsCommand { get => settingsCommand ?? (settingsCommand = new RelayCommand(Settings)); }

        public MainViewModel(IFileSaver<SaveObject> saver, IFileLoader<SaveObject> loader)
        {
            SaveObject.Instance = loader.Load(path);
            this.saver = saver;

        }

        void Host(object o)
        {
            try
            {
                NetworkHelper.GetLocalIPAddress();
                ViewLocator.Show<HostTemplateView>();
            }
            catch (Exception e)
            {
                ViewLocator.MainPage.DisplayAlert(string.Empty, e.Message, Lm.GetString("Ok"));
            }
        }

        async void Player(object o)
        {
            var vml = new ViewModelLocator();
            var res = await UserDialogs.Instance.PromptAsync(new PromptConfig()
            {
                Placeholder = Lm.GetString("InputName"),
                OnTextChanged = e => vml.PlayerViewModel.Player.Name = e.Value,
                Text = vml.PlayerViewModel.Player.Name,
                CancelText = Lm.Cancel
            });
            if (res.Ok)
            {
                if (vml.PlayerViewModel.Player.Name == string.Empty)
                    await ViewLocator.MainPage.DisplayAlert(string.Empty, Lm.GetString("NameNotEmpty"), Lm.GetString("Ok"));
                else
                    vml.PlayerViewModel.GoCommand.Execute(o);
            }
        }

        void Settings(object o) => ViewLocator.Show<SettingsView>();

        void Tutorial(object o)
        {
            var tutorialOptions = new List<ActionSheetOption>();
            tutorialOptions.Add(new ActionSheetOption(Lm.GetString("HowToPlay"), ViewLocator.Show<TutorialView>));
            tutorialOptions.Add(new ActionSheetOption(Lm.GetString("Roles"), ViewLocator.Show<RoleView>));
            UserDialogs.Instance.ActionSheet(new ActionSheetConfig()
            {
                Options = tutorialOptions,
                Cancel = new ActionSheetOption(Lm.Cancel)
            });
        }

        void IBack.Back() => Exit(null);

        async void Exit(object o)
        {
            var res = await ViewLocator.MainPage.DisplayAlert(string.Empty, Lm.GetString("ExitQ"), Lm.GetString("Yes"), Lm.GetString("No"));
            if (res)
            {
                Save();
                DependencyService.Get<ICloser>().Close();
            }
        }

        public void Save()
        {
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RCG")))
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RCG"));
            saver.Save(SaveObject.Instance, path);
        }
    }
}
