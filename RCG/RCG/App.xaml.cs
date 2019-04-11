using RCG.Infrastructure;
using RCG.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace RCG
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = ViewLocator.MainPage;
        }


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            new ViewModelLocator().MainViewModel.Save();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
