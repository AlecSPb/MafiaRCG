using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RCG.Droid.Dependency;
using RCG.Infrastructure;
using Xamarin.Forms;

[assembly: Dependency(typeof(GalleryPicker_Droid))]

namespace RCG.Droid.Dependency
{
    public class GalleryPicker_Droid : IGalleryPicker
    {
        public GalleryPicker_Droid() { }

        public Task<Stream> PickFromGallery()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);


            MainActivity.Instance.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Picture"),
                1000);

            MainActivity.Instance.PickImageTaskCompletionSource = new TaskCompletionSource<Stream>();

            return MainActivity.Instance.PickImageTaskCompletionSource.Task;
        }
    }
}