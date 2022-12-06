using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using XamarinARCore.Controller;

namespace XamarinARCore
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ARCoreController controllerAR;
        private TextView statusARcore;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            controllerAR = new ARCoreController(this);

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            statusARcore = FindViewById<TextView>(Resource.Id.tv_status_ar);

            bool isARCoreAvaliable = controllerAR.CheckARcore();


            if (isARCoreAvaliable)
			{
                statusARcore.Text = "ARCore is avaliable for this device!";
            }
			else
			{
                statusARcore.Text = "ARCore is NOT avaliable for this device!";
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}