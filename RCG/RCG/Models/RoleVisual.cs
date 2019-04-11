using RCG.Infrastructure;
using RCG.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace RCG.Models
{
    class RoleVisual : NotifiedBase
    {
        Role role;
        string roleStr;
        string roleDes;
        LocalizationManager lm;

        public string RoleStr { get => roleStr; set { roleStr = value; Notify(); } }
        public ImageSource Image { get; }
        public string RoleDes { get => roleDes; set { roleDes = value; Notify(); } }
        

        public RoleVisual(Role role)
        {
            this.role = role;
            lm = LocalizationManager.GetLocalizationManager();
            lm.PropertyChanged += CultureChanged;
            Image = ImageSource.FromResource(string.Concat("RCG.Resources.Pictures.", role.ToString(), ".jpg"), typeof(RoleVisual).Assembly);
            Update();

        }

        void CultureChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentCulture")
                Update();
        }

        void Update()
        {
            RoleStr = lm.GetString(role.ToString());
            RoleDes = lm.GetString(string.Concat(role.ToString(), "Des"));
        }

    }
}
