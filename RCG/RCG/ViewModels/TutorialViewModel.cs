using RCG.Infrastructure;
using RCG.Models;
using RCG.Models.Enums;
using RCG.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace RCG.ViewModels
{
    class TutorialViewModel : NotifiedBase, IBack
    {
        List<string> instructions;
        ICommand backCommand;

        public List<string> Instructions { get => instructions; set { instructions = value; Notify(); } }
        public List<RoleVisual> Roles { get; }
        public LocalizationManager Lm => LocalizationManager.GetLocalizationManager();
        public ICommand BackCommand { get => backCommand ?? (backCommand = new RelayCommand(Back)); }
        public ImageSource BGImage { get; } = ImageSource.FromResource("RCG.Resources.Pictures.Tutorial.jpg", typeof(MainViewModel).Assembly);

        public TutorialViewModel()
        {
            SetInstructions(5);
            Lm.PropertyChanged += CultureChanged;
            Roles = new List<RoleVisual>();
            foreach (var role in Enum.GetNames(typeof(Role)))
                if (role != Role.None.ToString())
                    Roles.Add(new RoleVisual((Role)Enum.Parse(typeof(Role), role)));
        }

        void SetInstructions(int n)
        {
            List<string> res = new List<string>();
            for (int i = 1; i <= n; i++)
                res.Add(Lm.GetString(string.Concat("Rule", i.ToString())));
            Instructions = res;
        }

        void CultureChanged(object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentCulture")
                SetInstructions(5);
        }

        void IBack.Back() => Back(null);

        void Back(object o) => ViewLocator.Show<MainView>();
    }
}
