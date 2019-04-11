using RCG.Infrastructure;
using RCG.Models.SaveLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RCG.Models
{
    class Settings : NotifiedBase
    {
        public string LanguageCode { get; set; } = LocalizationManager.GetLocalizationManager().CurrentCulture;


        Dictionary<string, string> availableLanguages = new Dictionary<string, string>()
        {
            { "English", "en" },
            { "Русский", "ru" },
            { "Українська", "uk"}
        };

        public List<string> AvailableLanguages => availableLanguages.Keys.ToList();

        public string Language
        {
            get => availableLanguages.Where(p => p.Value == LanguageCode).Single().Key;
            set
            {
                LanguageCode = availableLanguages[value];
                LocalizationManager.GetLocalizationManager().CurrentCulture = LanguageCode;
                Notify();
            }
        }

        public bool IsVibration { get; set; } = true;
    }
}
