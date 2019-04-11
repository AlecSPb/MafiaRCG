using RCG.Infrastructure;
using RCG.Models;
using RCG.Models.SaveLoad;
using RCG.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;

namespace RCG.ViewModels
{
    class HostTemplateViewModel : IBack
    {
        ICommand openCommand;
        ICommand cancelCommand;

        public ImageSource BackgroundImage { get; } = ImageSource.FromResource("RCG.Resources.Pictures.HDBackground.jpg", typeof(MainViewModel).Assembly);
        public HostTemplate HostTemplate => SaveObject.Instance.HostTemplate;
        public ICommand OpenCommand { get => openCommand ?? (openCommand = new RelayCommand(Open)); }
        public ICommand CancelCommand { get => cancelCommand ?? (cancelCommand = new RelayCommand(Cancel)); }
        public LocalizationManager Lm => LocalizationManager.GetLocalizationManager();

        public HostTemplateViewModel()
        {
        }

        void Open(object o)
        {
            if (HostTemplate.Name == string.Empty)
                ViewLocator.MainPage.DisplayAlert(string.Empty, Lm.GetString("RoomTitleNotEmpty"), Lm.GetString("Ok"));
            else
            {
                Host host = new Host(HostTemplate);
                host.Open();
                new ViewModelLocator().HostViewModel.Host = host;
                ViewLocator.Show<HostWaitingView>();
            }
        }

        void Cancel(object o) => ViewLocator.Show<MainView>();

        void IBack.Back() => Cancel(null);
    }
}
