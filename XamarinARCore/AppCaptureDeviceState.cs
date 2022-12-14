using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using static Android.Hardware.Camera2.CameraCaptureSession;

namespace XamarinARCore
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