using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.AR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamarinARCore.Helpers
{
	public class TrackingStateHelper
	{
		private static string INSUFFICIENT_FEATURES_MESSAGE = "Can't find anything. Aim device at a surface with more texture or color.";
		private static string EXCESSIVE_MOTION_MESSAGE = "Moving too fast. Slow down.";
		private static string INSUFFICIENT_LIGHT_MESSAGE = "Too dark. Try moving to a well-lit area.";
		private static string INSUFFICIENT_LIGHT_ANDROID_S_MESSAGE = "Too dark. Try moving to a well-lit area. Also, make sure the Block Camera is set to off in system settings.";
		private static string BAD_STATE_MESSAGE = "Tracking lost due to bad internal state. Please try restarting the AR experience.";
		private static string CAMERA_UNAVAILABLE_MESSAGE = "Another app is using the camera. Tap on this app or try closing the other one.";
		private static int ANDROID_S_SDK_VERSION = 31;

		private Activity activity;

		private TrackingState previousTrackingState;

		public TrackingStateHelper(Activity activity)
		{
			this.activity = activity;
		}

		/** Keep the screen unlocked while tracking, but allow it to lock when tracking stops. */
		public void updateKeepScreenOnFlag(TrackingState trackingState)
		{
			if (trackingState == previousTrackingState)
			{
				return;
			}

			previousTrackingState = trackingState;

			if (previousTrackingState == TrackingState.Stopped)
			{
				activity.RunOnUiThread(() => activity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn));
			}

			if (previousTrackingState == TrackingState.Stopped)
			{
				activity.RunOnUiThread(() => activity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn));
			}

		}

		public static String getTrackingFailureReasonString(Camera camera)
		{
			TrackingFailureReason reason = camera.TrackingFailureReason;

			if (reason == TrackingFailureReason.BadState)
			{
				return BAD_STATE_MESSAGE;
			}

			if (reason == TrackingFailureReason.InsufficientLight)
			{

				int SDK_INT = (int)Android.OS.Build.VERSION.SdkInt;
				if ( SDK_INT < ANDROID_S_SDK_VERSION)
				{
					return INSUFFICIENT_LIGHT_MESSAGE;
				}
				else
				{
					return INSUFFICIENT_LIGHT_ANDROID_S_MESSAGE;
				}
			}

			if (reason == TrackingFailureReason.ExcessiveMotion)
			{
				return EXCESSIVE_MOTION_MESSAGE;
			}			
			
			if (reason == TrackingFailureReason.InsufficientFeatures)
			{
				return INSUFFICIENT_FEATURES_MESSAGE;
			}

			if (reason == TrackingFailureReason.CameraUnavailable)
			{
				return CAMERA_UNAVAILABLE_MESSAGE;
			}

			return "Unknown tracking failure reason: " + reason;
		}
	}
}