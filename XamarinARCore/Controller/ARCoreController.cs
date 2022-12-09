using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.Snackbar;
using Google.AR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace XamarinARCore.Controller
{
	public class ARCoreController
	{
		private static string TAG = typeof(ARCoreController).Name;

		private Context context;
		private Session arSession;
		private bool requestInstallARCore = true;

		public ARCoreController(Context contextActivity)
		{
			context = contextActivity;
		}

		/// <summary>
		/// Verifica se o dispositivo é compartível com o ARCore.
		/// </summary>
		/// <returns></returns>
		public bool CheckARcore()
		{
			
			ArCoreApk.Availability isARCoreAvaliable = ArCoreApk.Instance.CheckAvailability(context);

			return isARCoreAvaliable.IsSupported;
		}

		public bool RequestInstallARCore(bool isDeviceSupported)
		{
			if (isDeviceSupported && arSession == null)
			{
				var isARCoreInstall = ArCoreApk.Instance.RequestInstall(context as Activity, requestInstallARCore);

				if (isARCoreInstall == ArCoreApk.InstallStatus.InstallRequested)
				{

					Log.Debug(TAG, "Solicitando instalação do app.");
					return true;

				}
				else if (isARCoreInstall == ArCoreApk.InstallStatus.Installed)
				{
					//Iniciando uma nova sessão do arcore.
					arSession = new Session(context);
					Log.Debug(TAG, "Iniciando a sessão!");
					return true;
				}
				else
				{
					Log.Debug(TAG, "Sessão não iniciada.");
					return false;
				}
			}
			else
			{
				Log.Debug(TAG, "Sessão já foi iniciada.");
				return false;
			}
		}

		public void CameraPermissionRequest()
		{
			// ARCore requires camera permission to operate.
			if (ContextCompat.CheckSelfPermission(context, Manifest.Permission.Camera) == (int)Permission.Granted)
			{
				//ActivityCompat.RequestPermissions(this, Permissions, REQUEST_LOCATION);
			}
		}
	}
}