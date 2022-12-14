using Android;
using Android.App;
using Android.Content.PM;
using Android.Hardware.Camera2;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Javax.Microedition.Khronos.Opengles;
using XamarinARCore.Camera;
using XamarinARCore.Controller.ARCore;

namespace XamarinARCore
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private string TAG = typeof(MainActivity).Name;

		public ARCoreController controllerAR;
		private TextureView cameraView;
		private TextView statusARcore;
		private AppCameraListener cameraListener;
		private AppCameraStateCallback appCallBack;
		private bool isARCoreAvaliable;
		private Handler mBackgroundHandler;

		private CameraManager cameraManager;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			controllerAR = new ARCoreController(this);

			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.activity_main);

			statusARcore = FindViewById<TextView>(Resource.Id.tv_status_ar);
			cameraView = FindViewById<TextureView>(Resource.Id.cameraView);

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

			InitCamera();
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

		private void InitCamera()
		{
			cameraListener = new AppCameraListener(this);
			cameraView.SurfaceTextureListener = cameraListener;
		}

		public void OpenCameraDevice()
		{
			cameraManager = (CameraManager)this.GetSystemService(CameraService);

			var cameraId = cameraManager.GetCameraIdList()[1];//camera frontal selecionada de forma manual.

			CameraCharacteristics cameraCharacteristics = cameraManager.GetCameraCharacteristics(cameraId);

			int backCamera = (int)cameraCharacteristics.Get(CameraCharacteristics.LensFacing);
			Log.Debug(TAG, "Camera selecionada: " + backCamera);

			appCallBack = new AppCameraStateCallback(cameraView);

			cameraManager.OpenCamera(cameraId, appCallBack, mBackgroundHandler);
		}

	}
}