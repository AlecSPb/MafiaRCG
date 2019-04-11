using Acr.UserDialogs;
using RCG.Infrastructure;
using RCG.Models;
using RCG.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace RCG.ViewModels
{
    class HostViewModel : NotifiedBase, IBack
    {
        Host host;
        ObservableCollection<Player> players;
        ICommand startCommand;
        ICommand stopCommand;
        ICommand nextCommand;
        ICommand actionCommand;
        ICommand resetCommand;
        ICommand devoteCommand;

        public ImageSource WaitingBackgroundImage { get; } = ImageSource.FromResource("RCG.Resources.Pictures.HostWaiting.jpg", typeof(MainViewModel).Assembly);
        public ImageSource BackgroundImage { get; } = ImageSource.FromResource("RCG.Resources.Pictures.City.jpg", typeof(MainViewModel).Assembly);
        public Host Host { get => host; set { host = value; Notify(); } }
        public ObservableCollection<Player> Players { get => players; set { players = value; Notify(); } }
        public ICommand StartCommand { get => startCommand ?? (startCommand = new RelayCommand(Start)); }
        public ICommand StopCommand { get => stopCommand ?? (stopCommand = new RelayCommand(Stop)); }
        public ICommand NextCommand { get => nextCommand ?? (nextCommand = new RelayCommand(Next)); }
        public ICommand ActionCommand { get => actionCommand ?? (actionCommand = new RelayCommand(Action, host.ActionPredicate)); }
        public ICommand ResetCommand { get => resetCommand ?? (resetCommand = new RelayCommand(Reset)); }
        public ICommand DevoteCommand { get => devoteCommand ?? (devoteCommand = new RelayCommand(Devote, o => (o as Player) == null ? true : (o as Player).VotedFor != 0)); }
        public LocalizationManager Lm => LocalizationManager.GetLocalizationManager();


        void Start(object o)
        {
            try
            {
                host.Start();
                host.PropertyChanged += ResStringChanged;
                Players = new ObservableCollection<Player>(host.Players);
                ViewLocator.Show<HostView>();
            }
            catch(Exception e)
            {
                ViewLocator.MainPage.DisplayAlert(string.Empty, e.Message, Lm.GetString("Ok"));
            }
        }

        void IBack.Back() => Stop(null);

        void Stop(object o)
        {
            host.Stop();
            ViewLocator.Show<MainView>();
        }

        void Next(object o)
        {
            try
            {
                host.NextPhase();
                (ActionCommand as RelayCommand).RaiseCanExecuteChanged();
                if (host.IsVote)
                    (DevoteCommand as RelayCommand).RaiseCanExecuteChanged();
            }
            catch (Exception e)
            {
                GameEnd(e.Message);
            }
        }

        async void GameEnd(string message)
        {
            await ViewLocator.MainPage.DisplayAlert(string.Empty, message, Lm.GetString("ToMainMenu"));
            ViewLocator.Show<MainView>();
        }

        void Action(object o)
        {
            try
            {
                host.Action(o as Player);
                (ActionCommand as RelayCommand).RaiseCanExecuteChanged();
                if(host.IsVote)
                    (DevoteCommand as RelayCommand).RaiseCanExecuteChanged();
            }
            catch (Exception e)
            {
                GameEnd(e.Message);
            }
        }

        void ResStringChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ResString")
                ViewLocator.MainPage.DisplayAlert(string.Empty, host.ResString, Lm.GetString("Ok"));
        }

        void Devote(object o)
        {
            host.Devote(o as Player);
            (DevoteCommand as RelayCommand).RaiseCanExecuteChanged();
            (ActionCommand as RelayCommand).RaiseCanExecuteChanged();
        }

        void Reset(object o)
        {
            Host.ResetVotes();
            (DevoteCommand as RelayCommand).RaiseCanExecuteChanged();
            (ActionCommand as RelayCommand).RaiseCanExecuteChanged();
        }
    }
}
