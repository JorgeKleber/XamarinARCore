using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.AR.Sceneform.Rendering;
using Google.AR.Sceneform.UX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using XamarinARCore.ARLibrary.Models;

namespace XamarinARCore.Views
{
    [Activity(Label = "GltfActivity")]
    public class GltfActivity : AppCompatActivity
    {

        private static string TAG = typeof(GltfActivity).Name;
        private static double MIN_OPENGL_VERSION = 3.0;

        private ArFragment arFragment;
        private Renderable renderable;

        private List<AnimationInstance> animatiors = new List<AnimationInstance>();
        private List<Color> colors = new List<Color>()
        {
         new Color(0, 0, 0, 1),
         new Color(1, 0, 0, 1),
         new Color(0, 1, 0, 1),
         new Color(0, 0, 1, 1),
         new Color(1, 1, 0, 1),
         new Color(0, 1, 1, 1),
         new Color(1, 0, 1, 1),
         new Color(1, 1, 1, 1),
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_ux);

        }
    }
}