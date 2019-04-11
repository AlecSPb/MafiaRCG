using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RCG.Droid.Dependency;
using RCG.Infrastructure;
using Xamarin.Forms;

[assembly: Dependency(typeof(Vibrate_Droid))]

namespace RCG.Droid.Dependency
{
    public class Vibrate_Droid : IVibrate
    {
        public Vibrate_Droid() { }


        public void Vibrate(int ms)
        {
            try
            {
                (Android.App.Application.Context.GetSystemService(Context.VibratorService) as Vibrator).Vibrate(ms);
            }
            catch(Exception e) { }
        }
    }
}