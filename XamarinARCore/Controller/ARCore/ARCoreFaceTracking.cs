using Google.AR.Core;
using System.Collections.Generic;
using System.Collections;
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

		/// <summary>
		///Face tracking apenas para identificar tudo o que foi rastreável.
		/// </summary>
		public void FaceTracking()
		{
			faces = new List<AugmentedFace>();

			var teste = session.GetAllTrackables(Java.Lang.Class.FromType(typeof(AugmentedFace)));

			//foreach (AugmentedFace item in session.GetAllTrackables(Java.Lang.Class.FromType(typeof(AugmentedFace))))
			//{
			//	faces.Add(item);
			//}

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