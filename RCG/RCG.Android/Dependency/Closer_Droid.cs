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
using RCG.Infrastructure;
using Xamarin.Forms;
using RCG.Droid.Dependency;


[assembly: Dependency(typeof(Closer_Droid))]
namespace RCG.Droid.Dependency
{
    public class Closer_Droid : ICloser
    {
        public Closer_Droid() { }

        public void Close()
        {
            Process.KillProcess(Process.MyPid());
        }
    }
}