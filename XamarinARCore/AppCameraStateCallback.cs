using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Icu.Util;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Android.Hardware.Camera2.CameraCaptureSession;

namespace XamarinARCore
{
	public class AppCameraStateCallback : CameraDevice.StateCallback
	{
		private CameraCharacteristics cameraCharacteristics;
		private TextureView tvView;
		public CaptureRequest.Builder Builder;
		private CameraDevice cameraDevice;
		private List<Surface> outputs = new List<Surface>();
		private AppCaptureDeviceState captureCallback;
		private Handler handler;

		public AppCameraStateCallback(CameraCharacteristics cameraCharacteristics, TextureView tvView )
		{
			this.cameraCharacteristics = cameraCharacteristics;
			this.tvView = tvView;

			captureCallback = new AppCaptureDeviceState(this);
		}

		public override void OnOpened(CameraDevice camera)
		{
			cameraDevice= camera;
			createpreview();
		}

		public override void OnDisconnected(CameraDevice camera)
		{
			//throw new NotImplementedException();
		}

		public override void OnError(CameraDevice camera, [GeneratedEnum] CameraError error)
		{
			//throw new NotImplementedException();
		}

		public void createpreview()
		{
			//HandlerThread thread = new HandlerThread("Camera Background");
			handler = new Handler();

			SurfaceTexture texture = tvView.SurfaceTexture;
			//texture.SetDefaultBufferSize(imagedimension.Width, imagedimension.Height);
			Surface surface = new Surface(texture);
			Builder = cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
			Builder.AddTarget(surface);
			outputs.Add(surface);
			cameraDevice.CreateCaptureSession(outputs, captureCallback, handler);
		}
	}
}