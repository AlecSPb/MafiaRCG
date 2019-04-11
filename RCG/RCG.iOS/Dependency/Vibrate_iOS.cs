using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioToolbox;
using Foundation;
using RCG.Infrastructure;
using RCG.iOS.Dependency;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Vibrate_iOS))]

namespace RCG.iOS.Dependency
{
    class Vibrate_iOS : IVibrate
    {
        public Vibrate_iOS() { }

        public void Vibrate(int ms)
        {
            SystemSound.Vibrate.PlayAlertSound();
        }
    }
}