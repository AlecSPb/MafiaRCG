using RCG.Views;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace RCG.Infrastructure
{
    public static class ViewLocator
    {
        public static ContentPage MainPage { get; } = new MainPage();

        static MainView mainView;
        static HostTemplateView hostTemplateView;
        static HostWaitingView hostWaitingView;
        static HostView hostView;
        static PlayerWaitingView playerWaitingView;
        static PlayerServerListView playerServerListView;
        static PlayerView playerView;
        static SettingsView settingsView;
        static TutorialView tutorialView;
        static RoleView roleView;

        static ViewLocator()
        {
            Show<MainView>();
        }

        async static void FadeAnimation(View view)
        {
            await MainPage.Content.FadeTo(0, 300);
            MainPage.Content = view;
            await MainPage.Content.FadeTo(1, 2000);
        }

        public static void Show<T>() where T: ContentView, new()
        {
            var fields = typeof(ViewLocator).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var field in fields)
                if(field.FieldType == typeof(T))
                {
                    var fieldValue = field.GetValue(null);
                    if (fieldValue == null)
                    {
                        T value = new T();
                        value.Opacity = 0;
                        field.SetValue(null, value);
                        fieldValue = value;
                    }
                    FadeAnimation((T)fieldValue);
                    break;
                }
        }

        public static void Prev() => (((MainPage.Content as ContentView).Content as Layout).BindingContext as IBack).Back();
    }
}
