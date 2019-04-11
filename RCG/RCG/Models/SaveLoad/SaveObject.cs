using System;
using System.Collections.Generic;
using System.Text;

namespace RCG.Models.SaveLoad
{
    class SaveObject
    {
        static SaveObject saveObject;
        public static SaveObject Instance
        {
            get => saveObject;
            set
            {
                if (saveObject == null)
                    saveObject = value;
            }
        }

        public string PlayerName { get; set; } = string.Empty;
        public HostTemplate HostTemplate { get; set; } = new HostTemplate();
        public Settings Settings { get; set; } = new Settings();
    }
}
