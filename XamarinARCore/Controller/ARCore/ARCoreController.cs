using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Webkit;
using AndroidX.Core.Content;
using Google.AR.Core;
using Google.AR.Core.Exceptions;
using Java.Util;
using System.Collections.Generic;
using System.Linq;
using static Google.AR.Core.Config;

namespace XamarinARCore.Controller.ARCore
{
	public class ARCoreController
	{
		private static string TAG = typeof(ARCoreController).Name;

		private Context context;
		private Session arSession;
		private bool requestInstallARCore = true;
		public ARCoreFaceTracking faceTracking;

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
					arSession = CreateNewARCoreSession(ARmode.AugmentedFace);

					faceTracking = new ARCoreFaceTracking(arSession);
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
		private Session CreateNewARCoreSession(ARmode aRmode)
		{
			Session newSession = new Session(context);
			Log.Debug(TAG, "Sessão criada!");

			Google.AR.Core.Config config = SetARConfig(newSession);
			CameraConfig cameraConfig = SetCameraConfig(newSession, aRmode);

			newSession.Configure(config);
			newSession.CameraConfig = cameraConfig;
			Log.Debug(TAG, "Configuração definida!!!");

			return newSession;
		}

		private Google.AR.Core.Config SetARConfig(Session session)
		{
			Google.AR.Core.Config config = new Google.AR.Core.Config(session);
			Log.Debug(TAG, "Configuração criada com sucesso!");

			//configurando o foco da camera, Fixed é o padrão adotado na maioria dos dispositivos.
			config.SetFocusMode(Google.AR.Core.Config.FocusMode.Fixed);
			Log.Debug(TAG, "Foco da camera configurado!");

			return config;
		}

		/// <summary>
		/// Retorna as configurações da camera.
		/// </summary>
		/// <param name="currentSession"></param>
		/// <returns></returns>
		private CameraConfig SetCameraConfig(Session session, ARmode modeAR)
		{
			if (arSession == null)
			{
				Log.Debug(TAG, "Sessão não iniciada.");
			}

			Log.Debug(TAG, "Configurando preferencias da camera.");

			CameraConfigFilter cameraConfigFilter;
			CameraConfig cameraConfigList;

			switch (modeAR)
			{
				case ARmode.AugmentedReality:

					cameraConfigFilter = new CameraConfigFilter(session);
					cameraConfigList = arSession.GetSupportedCameraConfigs(cameraConfigFilter)[0];

					break;
				case ARmode.AugmentedFace:

					cameraConfigFilter = new CameraConfigFilter(session).SetFacingDirection(CameraConfig.FacingDirection.Front);

					cameraConfigList = session.GetSupportedCameraConfigs(cameraConfigFilter)[0];

					break;

				default:

					cameraConfigFilter = new CameraConfigFilter(session);
					cameraConfigList = arSession.GetSupportedCameraConfigs(cameraConfigFilter)[0];

					break;
			}

			//Limita o frame rate da captura da camera em 30 fps(quadros por segundo).
			cameraConfigFilter.SetTargetFps(EnumSet.Of(CameraConfig.TargetFps.TargetFps30));

			//Retorna apenas as configurações da camera que não usam depth sensor.(Sensor de profundidade).
			cameraConfigFilter.SetDepthSensorUsage(EnumSet.Of(CameraConfig.DepthSensorUsage.DoNotUse));

			return cameraConfigList;


		}

		/// <summary>
		/// Obtendo metadatas do frame da camera.
		/// </summary>
		/// <param name="frame"></param>
		/// <returns></returns>
		public long GetMetaDataSensorSensitivity(Frame frame)
		{
			try
			{
				ImageMetadata metadata = frame.ImageMetadata;

				return metadata.GetLong(ImageMetadata.SensorSensitivity);
			}
			catch (MetadataNotFoundException exception)
			{
				Log.Debug(TAG, exception.Message);
				return 0;
			}
		}

		/// <summary>
		/// Encerrando a sessão criada.
		/// </summary>
		public void CloseARSession()
		{
			Log.Debug(TAG, "Sessão finalizada com sucesso!");
			arSession.Close();
			arSession = null;
		}
	}

	public enum ARmode
	{
		AugmentedReality,
		AugmentedFace,
	}
}