using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.AR.Core;
using Google.AR.Sceneform;
using Google.AR.Sceneform.Assets;
using Google.AR.Sceneform.Rendering;
using Google.AR.Sceneform.UX;
using Xamarin.Essentials;
using XamarinARCore.Controller.ARCore;

namespace XamarinARCore
{
    [Activity(Label = "ArActivity", Theme = "@style/AppTheme", MainLauncher = true)]
    public class ArActivity : Android.Support.V7.App.AppCompatActivity
    {
        private FrameLayout arFragment;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ar_activity);

            // Obter a referência ao ArFragment
            arFragment = FindViewById<FrameLayout>(Resource.Id.ar_fragment);

            var _arFragment = new FaceARFragment();

            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.ar_fragment, _arFragment);
            transaction.Commit();
        }
    }
}