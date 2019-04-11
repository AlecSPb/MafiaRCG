using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation;
using RCG.Infrastructure;
using UIKit;
using Xamarin.Forms;
using RCG.iOS.Dependency;

[assembly: Dependency(typeof(Closer_iOS))]


namespace RCG.iOS.Dependency
{
    public class Closer_iOS : ICloser
    {
        public Closer_iOS() { }

        public void Close()
        {
            Thread.CurrentThread.Abort();
        }
    }
}