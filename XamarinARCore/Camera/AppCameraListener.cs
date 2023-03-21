using Android.Content.PM;
using Android.Graphics;
using Android.Util;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using XamarinARCore.Controller.ARCore;
using static Android.Views.TextureView;

namespace XamarinARCore.Camera
{
	public class AppCameraListener : Java.Lang.Object, ISurfaceTextureListener
	{
		private string TAG = typeof(AppCameraListener).Name;
		private readonly MainActivity activity;

		public AppCameraListener(MainActivity activity)
		{
			this.activity = activity;
		}


		public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
		{
			Log.Debug(TAG, "OnSurfaceTextureAvailable inicializada!");
			activity.OpenCameraDevice();
		}

		public async void CameraPermissionCheck()
		{
            Log.Debug(TAG, "Verificando permissões da camera...");

            var status = await Xamarin.Essentials.Permissions.CheckStatusAsync<Permissions.Camera>();

			if (status != PermissionStatus.Granted)
			{
				await Permissions.RequestAsync<Permissions.Camera>();
			}
			
		}

		public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
		{
			return true;
		}

		public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
		{
			//Log.Debug(TAG, "OnSurfaceTextureAvailable inicializada!");
		}

		public void OnSurfaceTextureUpdated(SurfaceTexture surface)
		{
			//Log.Debug(TAG, "OnSurfaceTextureAvailable inicializada!");
		}
	}
}