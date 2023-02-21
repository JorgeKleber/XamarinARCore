using Android.Graphics;
using Android.Util;
using System;
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