using Acr.UserDialogs;
using RCG.Infrastructure;
using RCG.Models;
using RCG.Models.SaveLoad;
using RCG.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Input;
using Xamarin.Forms;
using RCG.Models.Enums;
using System.Threading.Tasks;

namespace RCG.ViewModels
{
    class PlayerViewModel : NotifiedBase, IBack
    {
        PlayerClient player;
        bool refreshing = false;
        double bloodOpacity = 0;
        bool killedVisible = false;
        ImageSource roleImage;
        ICommand goCommand;
        ICommand connectCommand;
        ICommand refreshServersCommand;
        ICommand backCommand;

        public ImageSource BloodImg { get; } = ImageSource.FromResource("RCG.Resources.Pictures.Blood.png", typeof(MainViewModel).Assembly);
        public ImageSource ServerBG { get; } = ImageSource.FromResource("RCG.Resources.Pictures.ServerList.jpg", typeof(MainViewModel).Assembly);
        public ImageSource RoleImage { get => roleImage; set { roleImage = value; Notify(); } } 
        public ImageSource BackgroundImage { get; } = ImageSource.FromResource("RCG.Resources.Pictures.WaitBackground.jpg", typeof(MainViewModel).Assembly);
        public string Password { get; set; } = string.Empty;
        public bool Refreshing { get => refreshing; set { refreshing = value; Notify(); } }
        public double BloodOpacity { get => bloodOpacity; set { bloodOpacity = value; Notify(); } }
        public bool KilledVisible { get => killedVisible; set { killedVisible = value; Notify(); } }
        public PlayerClient Player { get => player; set { player = value; Notify(); } }
        public ICommand GoCommand { get => goCommand ?? (goCommand = new RelayCommand(Go)); }
        public ICommand ConnectCommand { get => connectCommand ?? (connectCommand = new RelayCommand(Connect)); }
        public ICommand RefreshServersCommand { get => refreshServersCommand ?? (refreshServersCommand = new RelayCommand(RefreshServers)); }
        public ICommand BackCommand { get => backCommand ?? (backCommand = new RelayCommand(Back)); }
        public LocalizationManager Lm => LocalizationManager.GetLocalizationManager();
        public string CountStr => string.Concat(Lm.GetString("PlayersConnected"), " - ", player.PlayerCount.ToString());
        public string RoleStr => string.Concat(Lm.GetString(Player.Role.ToString()));
        public string AliveStr => string.Concat(Lm.GetString("Alive"), " - ", player.PlayerCount.ToString());


        public PlayerViewModel()
        {
            Player = new PlayerClient();
            Player.PropertyChanged += PlayerPropertyChanged;
        }

        void Go(object o)
        {
            Refreshing = true;
            ThreadPool.QueueUserWorkItem(Refresh);
            ViewLocator.Show<PlayerServerListView>();
        }


        async void Connect(object o)
        {
            string hostName = (o as ItemTappedEventArgs).Item.ToString();
            try
            {
                var res = await UserDialogs.Instance.PromptAsync(new PromptConfig()
                {
                    Placeholder = Lm.GetString("InputPass"),
                    OnTextChanged = e => Password = e.Value,
                    CancelText = Lm.Cancel
                });
                if (res.Ok)
                {
                    Player.Connect(hostName, Password);
                    ViewLocator.Show<PlayerWaitingView>();
                }
            }
            catch (Exception e)
            {
                await ViewLocator.MainPage.DisplayAlert(string.Empty, e.Message, Lm.GetString("Ok"));
            }
        }

        void PlayerPropertyChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Role")
            {
                BloodOpacity = 0;
                KilledVisible = false;
                Notify("RoleStr");
                RoleImage = ImageSource.FromResource(string.Concat("RCG.Resources.Pictures.", Player.Role.ToString(), ".jpg"), typeof(MainViewModel).Assembly);
                Device.BeginInvokeOnMainThread(ViewLocator.Show<PlayerView>);
            }
            else if (e.PropertyName == "PlayerCount")
            {
                Notify("CountStr");
                Notify("AliveStr");
            }
            else if(e.PropertyName == "Status")
            {
                switch (player.Status)
                {
                    case Status.MafiaWon:
                    case Status.CitizensWon:
                    case Status.AssassinWon:
                        Device.BeginInvokeOnMainThread(() => ViewLocator.MainPage.DisplayAlert(string.Empty, Lm.GetString(player.Status.ToString()), Lm.GetString("Ok")));
                        Device.BeginInvokeOnMainThread(() => Back(null));
                        break;
                    case Status.Killed:
                        if (SaveObject.Instance.Settings.IsVibration)
                            DependencyService.Get<IVibrate>().Vibrate(1000);
                        FadeAsync();
                        KilledVisible = true;
                        break;
                    case Status.Disconnected:
                        Device.BeginInvokeOnMainThread(() => ViewLocator.MainPage.DisplayAlert(string.Empty, Lm.GetString("ServerStoped"), Lm.GetString("Ok")));
                        Device.BeginInvokeOnMainThread(() => Back(null));
                        break;
                }
            }
        }

        async void FadeAsync() => await Task.Run(() => Fade());

        void Fade()
        {
            for (int i = 0; i < 20; i++)
            {
                BloodOpacity += 0.025;
                Thread.Sleep(50);
            }
        }

        void RefreshServers(object o)
        {
            Refreshing = true;
            ThreadPool.QueueUserWorkItem(Refresh);
        }

        void Refresh(object o)
        {
            try
            {
                Player.Refresh();
                Refreshing = false;
                if (Player.ServerList.Count == 0)
                    RefreshServers(o);
            }
            catch (Exception e)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ViewLocator.MainPage.DisplayAlert(string.Empty, e.Message, Lm.GetString("Ok"));
                    ViewLocator.Show<MainView>();
                });
            }
        }

        void IBack.Back() => Back(null);

        void Back(object o)
        {
            Player.Disconnect();
            ViewLocator.Show<MainView>();
        }
    }
}
