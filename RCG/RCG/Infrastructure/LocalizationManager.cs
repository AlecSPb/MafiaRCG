//using Foundation;
using Java.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly:NeutralResourcesLanguage("en")]

namespace RCG.Infrastructure
{
    public class LocalizationManager : NotifiedBase
    {
        public string Host => GetString();
        public string Player => GetString();
        public string Tutorial => GetString();
        public string Exit => GetString();
        public string RoomTitle => GetString();
        public string RoomPass => GetString();
        public string Doctor => GetString();
        public string Detective => GetString();
        public string Girl => GetString();
        public string Assassin => GetString();
        public string Immortal => GetString();
        public string Don => GetString();
        public string OpenRoom => GetString();
        public string Cancel => GetString();
        public string StartGame => GetString();
        public string Stop => GetString();
        public string Next => GetString();
        public string Reset => GetString();
        public string Disconnect => GetString();
        public string Back => GetString();
        public string Settings => GetString();
        public string Language => GetString();
        public string KilledPl => GetString();
        public string AttImage => GetString();
        public string Send => GetString();
        public string Message => GetString();
        public string DeathVibr => GetString();

        static LocalizationManager localizationManager;
        public static LocalizationManager GetLocalizationManager()
        {
            if (localizationManager == null)
                localizationManager = new LocalizationManager();
            return localizationManager;
        }

        public string CurrentCulture
        {
            get => Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            set
            {
                try
                {
                    CultureInfo cultureInfo = new CultureInfo(value);
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Notify();
                }
                catch { }
            }
        }

        static readonly ResourceManager resourceManager = new ResourceManager("RCG.Resources.Localization.Strings", typeof(LocalizationManager).Assembly);

        public string Text { get; set; }

        LocalizationManager()
        {
            PropertyChanged += PropertyChangedHandler;
        }

        void PropertyChangedHandler(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentCulture")
            {
                foreach (var prop in typeof(LocalizationManager).GetProperties())
                    if (prop.Name != "CurrentCulture")
                        Notify(prop.Name);
            }
        }

        public string GetString([CallerMemberName]string str = "")
        {
            try { return resourceManager.GetString(str); }
            catch { return string.Empty; }
        }
    }
}
