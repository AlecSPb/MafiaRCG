using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using RCG.Infrastructure;
using RCG.Main.Models;
using RCG.Main.Models.SaveLoad;
using RCG.Views;
using RCG.Main.Infrastructure;
using Xamarin.Forms;

namespace RCG.ViewModels
{
    class SettingsViewModel : NotifiedBase, IBack
    {
        ICommand backCommand;
        ICommand languageCommand;
        ICommand reportCommand;
        ICommand chooseScreenshotCommand;
        ICommand chooseSubjCommand;
        ICommand hideCommand;
        IList<ActionSheetOption> sheetOptions;
        bool reportVisible = false;
        string subj;
        bool sendEnabled = true;
        Stream attachment;
        ImageSource bgImage = ImageSource.FromResource("RCG.Main.Resources.Pictures.Settings.jpg", typeof(Host).Assembly);

        public ImageSource BGImage { get => bgImage; set { bgImage = value; Notify(); } } 
        public Settings Settings => SaveObject.Instance.Settings;
        public LocalizationManager Lm => LocalizationManager.GetLocalizationManager();
        public bool ReportVisible { get => reportVisible; set { reportVisible = value; Notify(); } }
        public string ReportBtnStr { get => ReportVisible ? Lm.GetString("Hide") : Lm.GetString("ReportTitle"); }
        public string Subj { get => Lm.GetString(subj); }
        public string Body { get; set; } = string.Empty;
        public bool SendEnabled { get => sendEnabled; set { sendEnabled = value; (ReportCommand as RelayCommand).RaiseCanExecuteChanged(); (ChooseScreenshotCommand as RelayCommand).RaiseCanExecuteChanged(); } }
        public Color AttColor { get => attachment == null ? Color.DarkRed : Color.Green; }
        public ICommand BackCommand { get => backCommand ?? (backCommand = new RelayCommand(Back)); }
        public ICommand LanguageCommand { get => languageCommand ?? (languageCommand = new RelayCommand(Language)); }
        public ICommand ReportCommand { get => reportCommand ?? (reportCommand = new RelayCommand(Report, o => SendEnabled)); }
        public ICommand ChooseScreenshotCommand { get => chooseScreenshotCommand ?? (chooseScreenshotCommand = new RelayCommand(ChooseScreenshot, o => SendEnabled)); }
        public ICommand ChooseSubjCommand { get => chooseSubjCommand ?? (chooseSubjCommand = new RelayCommand(ChooseSubj)); }
        public ICommand HideCommand { get => hideCommand ?? (hideCommand = new RelayCommand(Hide)); }

        public SettingsViewModel()
        {
            Lm.PropertyChanged += CultureChanged;
            subj = "Report";
        }

        void Back(object o) => ViewLocator.Show<MainView>();

        void IBack.Back() => Back(null);

        void Language(object o)
        {
            if (sheetOptions == null)
            {
                sheetOptions = new List<ActionSheetOption>();
                foreach (var lang in Settings.AvailableLanguages)
                    sheetOptions.Add(new ActionSheetOption(lang, () => Settings.Language = lang));
            }
            UserDialogs.Instance.ActionSheet(new ActionSheetConfig()
            {
                Options = sheetOptions,
                Cancel = new ActionSheetOption(Lm.Cancel)
            });
        }

        async void Report(object o)
        {
            if (NetworkHelper.CheckNetwork())
            {
                var dis = UserDialogs.Instance.Loading(Lm.GetString("Sending"));
                await NetworkHelper.SendMailAsync(Subj, Body, attachment);
                dis.Dispose();
                Hide(o);
            }
            else
                await ViewLocator.MainPage.DisplayAlert(string.Empty, Lm.GetString("NoInternet"), Lm.Cancel);

        }

        async void ChooseScreenshot(object o)
        {
            SendEnabled = false;
            attachment = await DependencyService.Get<IGalleryPicker>().PickFromGallery();
            await Task.Run(() => ConvertToMS());
            Notify("AttColor");
            SendEnabled = true;
        }

        void ConvertToMS()
        {
            if (attachment != null)
            {
                List<byte> bytes = new List<byte>(500000);
                byte[] buff = new byte[1024];
                while (attachment.Read(buff, 0, 1024) > 0)
                    bytes.AddRange(buff);
                attachment = new MemoryStream(bytes.ToArray());
            }
        }

        void Hide(object o)
        {
            if (ReportVisible)
            {
                ReportVisible = false;
                attachment?.Close();
                attachment = null;
                Body = string.Empty;
                Notify("Body");
                Notify("AttColor");
            }
            else
                ReportVisible = true;
            Notify("ReportBtnStr");
        }

        void ChooseSubj(object o)
        {
            var reportSheetOptions = new List<ActionSheetOption>();
            reportSheetOptions.Add(new ActionSheetOption(Lm.GetString("Report"), () => { subj = "Report"; Notify("Subj"); }));
            reportSheetOptions.Add(new ActionSheetOption(Lm.GetString("Wish"), () => { subj = "Wish"; Notify("Subj"); }));
            UserDialogs.Instance.ActionSheet(new ActionSheetConfig()
            {
                Options = reportSheetOptions,
                Cancel = new ActionSheetOption(Lm.Cancel)
            });
        }

        void CultureChanged(object o, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "CurrentCulture")
            {
                Notify("ReportBtnStr");
                Notify("Subj");
            }
        }
    }
}
