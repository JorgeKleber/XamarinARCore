using Google.AR.Core;
using Org.Apache.Http.Impl.IO;
using System.Collections.Generic;

namespace XamarinARCore.Controller.ARCore
{
	public class ARCoreFaceTracking
	{
		private static string TAG = typeof(ARCoreFaceTracking).Name;

		//private List<AugmentedFace> faces;
		private List<AugmentedFace> faces;
		private Session session;

		public ARCoreFaceTracking(Session session)
		{
			this.session = session;	
		}

		public void FaceTracking()
		{
			faces = new List<AugmentedFace>();

			foreach (AugmentedFace item in session.GetAllTrackables(Java.Lang.Class.FromType(typeof(AugmentedFace))))
			{
				faces.Add(item);
			}

			//foreach (AugmentedFace face in faces)
			//{
			//	if (face.TrackingState == TrackingState.Tracking)
			//	{
			//		Log.Debug(TAG, "FACE ENCONTRADAAAA!!!!!!!!");
			//	}
			//}
		}
	}
}