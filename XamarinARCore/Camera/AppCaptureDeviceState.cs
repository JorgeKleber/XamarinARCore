using Android.Hardware.Camera2;
using Android.OS;
using Android.Util;
using System;
using XamarinARCore.Controller.ARCore;
using static Android.Hardware.Camera2.CameraCaptureSession;

namespace XamarinARCore.Camera
{
	public class AppCaptureDeviceState : StateCallback
	{
        private static string TAG = typeof(AppCaptureDeviceState).Name;

        public  CameraCaptureSession cameraCaptureSessions;
		private AppCameraStateCallback stateCallback;
		private Handler handler;

		public AppCaptureDeviceState(AppCameraStateCallback stateCallback)
		{
			this.stateCallback = stateCallback;
		}

		public override void OnConfigured(CameraCaptureSession session)
		{
			cameraCaptureSessions = session;
			UpdatePreview();
		}

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			throw new NotImplementedException();
		}

		private void UpdatePreview()
		{
			Log.Debug(TAG, "Iniciando UpdatePreview");

			//HandlerThread thread = new HandlerThread("Camera Background");
			handler = new Handler();
			cameraCaptureSessions.SetRepeatingRequest(stateCallback.Builder.Build(), null, handler);

		}
	}
}