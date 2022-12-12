using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using XamarinARCore.Controller;

namespace XamarinARCore
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private string TAG = typeof(MainActivity).Name;

		private ARCoreController controllerAR;
		private TextView statusARcore;
		private bool isARCoreAvaliable;


		protected override void OnCreate(Bundle savedInstanceState)
		{
			controllerAR = new ARCoreController(this);

			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			statusARcore = FindViewById<TextView>(Resource.Id.tv_status_ar);

			isARCoreAvaliable = controllerAR.CheckARcore();


			if (isARCoreAvaliable)
			{
				statusARcore.Text = "ARCore is avaliable for this device!";

				if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == (int)Permission.Granted)
				{
					Log.Debug(TAG, "PERMISSION GRANTED!!!");
					controllerAR.RequestInstallARCore(isARCoreAvaliable);
				}
				else
				{
					ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.Camera }, 0);
				}
			}
			else
			{
				statusARcore.Text = "ARCore is NOT avaliable for this device!";
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		protected override void OnDestroy()
		{
			controllerAR.CloseARSession();
			base.OnDestroy();
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			controllerAR.RequestInstallARCore(isARCoreAvaliable);
		}
	}
}