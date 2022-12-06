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

namespace XamarinARCore.Controller
{
	public class ARCoreController
	{
		private Context context;

		public ARCoreController(Context contextActivity)
		{
			context = contextActivity;
		}


		public bool CheckARcore()
		{
			ArCoreApk.Availability isARCoreAvaliable = ArCoreApk.Instance.CheckAvailability(context);

			return isARCoreAvaliable.IsSupported;
		}
	}
}