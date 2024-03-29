﻿using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;
using System.Collections.Generic;
using XamarinARCore.Controller.ARCore;

namespace XamarinARCore.Camera
{
	public class AppCameraStateCallback : CameraDevice.StateCallback
	{

        private static string TAG = typeof(AppCameraStateCallback).Name;

        private TextureView tvView;
		public CaptureRequest.Builder Builder;
		private CameraDevice cameraDevice;
		private List<Surface> outputs = new List<Surface>();
		private AppCaptureDeviceState captureCallback;
		private Handler handler;
		public ARCoreController arCoreController;

        public AppCameraStateCallback(TextureView tvView, ARCoreController arCoreController)
		{
			this.tvView = tvView;

			captureCallback = new AppCaptureDeviceState(this);
			this.arCoreController = arCoreController;
        }

		public override void OnOpened(CameraDevice camera)
		{
			cameraDevice = camera;
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
			HandlerThread thread = new HandlerThread("Camera Background");
			handler = new Handler();

			SurfaceTexture texture = tvView.SurfaceTexture;
			texture.SetDefaultBufferSize(1920, 1080); //Setado manualmente.
			Surface surface = new Surface(texture);
			Builder = cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
			Builder.AddTarget(surface);
			outputs.Add(surface);
			cameraDevice.CreateCaptureSession(outputs, captureCallback, handler);
		}
	}
}