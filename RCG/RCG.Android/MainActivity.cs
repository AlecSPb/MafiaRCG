using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using RCG.Infrastructure;
using Acr.UserDialogs;
using Java.Util;
using Android.Support.V4.App;
using Android;
using Android.Support.V4.Content;
using Xamarin.Forms;
using Android.Content;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace RCG.Droid
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, Label = "RCG", Icon = "@drawable/Icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance;

        public MainActivity()
        {
            Instance = this;
        }

        public override void OnBackPressed()
        {
            ViewLocator.Prev();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            base.OnCreate(savedInstanceState);
            Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 101);
            while (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            { }
            //LocalizationManager.GetLocalizationManager().CurrentCulture = Locale.Default.ToString().Replace('_', '-');
            LoadApplication(new App());
            UserDialogs.Init(this);
        }

        public TaskCompletionSource<Stream> PickImageTaskCompletionSource { get; set; }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(requestCode == 1000)
            {
                if (resultCode == Result.Ok && data != null)
                {
                    var res = ContentResolver.OpenInputStream(data.Data);
                    PickImageTaskCompletionSource.SetResult(res);
                }
                else
                    PickImageTaskCompletionSource.SetResult(null);
            }
        }
    }
}