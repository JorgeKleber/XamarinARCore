using Android.Hardware.Camera2;
using Android.OS;
using System;
using static Android.Hardware.Camera2.CameraCaptureSession;

namespace XamarinARCore.Camera
{
	public class AppCaptureDeviceState : StateCallback
	{
		public CameraCaptureSession cameraCaptureSessions;
		private AppCameraStateCallback stateCallback;
		private Handler handler;

		public AppCaptureDeviceState(AppCameraStateCallback stateCallback)
		{
			this.stateCallback = stateCallback;
		}

		public override void OnConfigured(CameraCaptureSession session)
		{
			cameraCaptureSessions= session;
			UpdatePreview();
		}

		public override void OnConfigureFailed(CameraCaptureSession session)
		{
			throw new NotImplementedException();
		}

		private void UpdatePreview()
		{
			//HandlerThread thread = new HandlerThread("Camera Background");
			handler = new Handler();
			cameraCaptureSessions.SetRepeatingRequest(stateCallback.Builder.Build(), null, handler);
		}
	}
}