using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using AndroidX.Core.Content;
using Google.AR.Core;
using Java.Util;
using System.Collections.Generic;

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

		/// <summary>
		/// Verifica se o app arcore está instalado.
		/// </summary>
		/// <param name="isDeviceSupported"></param>
		/// <returns></returns>
		public void RequestInstallARCore(bool isDeviceSupported)
		{
			if (isDeviceSupported && arSession == null)
			{
				var isARCoreInstall = ArCoreApk.Instance.RequestInstall(context as Activity, requestInstallARCore);

				if (isARCoreInstall == ArCoreApk.InstallStatus.InstallRequested)
				{
					Log.Debug(TAG, "Solicitando instalação do app.");
				}
				else if (isARCoreInstall == ArCoreApk.InstallStatus.Installed)
				{
					arSession = CreateNewARCoreSession();
				}
				else
				{
					Log.Debug(TAG, "Sessão não iniciada.");
				}
			}
			else
			{
				Log.Debug(TAG, "Sessão já foi iniciada.");
			}
		}

		/// <summary>
		/// Criando uma nova sessão ARCore.
		/// </summary>
		private Session CreateNewARCoreSession()
		{
			Session newSession = new Session(context);
			Log.Debug(TAG, "Sessão criada!");

			Google.AR.Core.Config config = new Google.AR.Core.Config(newSession);
			Log.Debug(TAG, "Configuração criada com sucesso!");

			//configurando o foco da camera, Fixed é o padrão adotado na maioria dos dispositivos.
			config.SetFocusMode(Google.AR.Core.Config.FocusMode.Fixed);
			Log.Debug(TAG, "Foco da camera configurado!");

			newSession.Configure(config);
			Log.Debug(TAG, "Configuração definida!!!");

			
			return newSession;
		}

		/// <summary>
		/// Retorna as configurações da camera.
		/// </summary>
		/// <param name="currentSession"></param>
		/// <returns></returns>
		private CameraConfig SetCameraConfig(Session currentSession)
		{
			Log.Debug(TAG, "Configurando preferencias da camera.");

			
			CameraConfigFilter cameraConfigFilter = new CameraConfigFilter(currentSession);

			//Limita o frame rate da captura da camera em 30 fps(quadros por segundo).
			cameraConfigFilter.SetTargetFps(EnumSet.Of(CameraConfig.TargetFps.TargetFps30));

			//Retorna apenas as configurações da camera que não usam depth sensor.(Sensor de profundidade).
			cameraConfigFilter.SetDepthSensorUsage(EnumSet.Of(CameraConfig.DepthSensorUsage.DoNotUse));

			List<CameraConfig> cameraConfigList = (List<CameraConfig>)currentSession.GetSupportedCameraConfigs(cameraConfigFilter);

			return cameraConfigList[0];

		}

		public void CloseARSession()
		{
			Log.Debug(TAG, "Sessão finalizada com sucesso!");
			arSession.Close();
			arSession = null;
		}
	}
}