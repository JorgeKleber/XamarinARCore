using Android.App;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using Android.Opengl;
using Google.AR.Core;
using Javax.Microedition.Khronos.Opengles;
using AndroidX.Core.Content;
using AndroidX.Core.App;
using Google.Android.Material.Snackbar;
using System;
using XamarinARCore.Rendering;
using XamarinARCore.Helpers;
using System.Collections.ObjectModel;
using static Google.AR.Core.AugmentedFace;
using Android.Util;

namespace XamarinARCore
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, GLSurfaceView.IRenderer
	{
		#region CONFIGURATIONS/INITIALIZING

		private static string TAG = typeof(MainActivity).Name;

		//renderizador. As renderizações são criadas usando este objeto e inicializado quando o surfaceview é criado.
		/// <summary>
		/// Renderizador - Um componente de UI para renderizar imagens na tela.
		/// </summary>
		private GLSurfaceView surfaceView;

		private bool installRequested;

		//responsável por iniciar a sessão.
		private Session session;
		//private SnackbarHelper messageSnackbarHelper = new SnackbarHelper();
		private DisplayRotationHelper displayRotationHelper;
		private TrackingStateHelper trackingStateHelper = new TrackingStateHelper(Android.App.Application.Context as Activity);

		//Instanciando os renderizadores.
		private BackgroundRenderer backgroundRenderer = new BackgroundRenderer();
		private AugmentedFaceRenderer augmentedFaceRenderer = new AugmentedFaceRenderer();
		private ObjectRenderer noseObject = new ObjectRenderer();
		private ObjectRenderer rightEarObject = new ObjectRenderer();
		private ObjectRenderer leftEarObject = new ObjectRenderer();


		//Temporary matrix allocated here to reduce number of allocations for each frame.
		private float[] noseMatrix = new float[16];
		private float[] rightEarMatrix = new float[16];
		private float[] leftEarMatrix = new float[16];
		private static float[] DEFAULT_COLOR = new float[] { 0f, 0f, 0f, 0f };

		#endregion

		#region ANDROID LIFE CIRCLE

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			//referenciado o renderizador.
			surfaceView = FindViewById<GLSurfaceView>(Resource.Id.surfaceview);

			//configurando o renderizador
			surfaceView.PreserveEGLContextOnPause = true;
			surfaceView.SetEGLContextClientVersion(2);
			surfaceView.SetEGLConfigChooser(8, 8, 8, 8, 16, 0);
			surfaceView.SetRenderer(this);
			surfaceView.RenderMode = Rendermode.Continuously;
			surfaceView.SetWillNotDraw(false);

			installRequested = false;
		}

		protected override void OnResume()
		{
			base.OnResume();

			// ARCore requires camera permissions to operate. If we did not yet obtain runtime
			// permission on Android M and above, now is a good time to ask the user for it.
			if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted)
			{
				if (session != null)
				{
					showLoadingMessage();
					// Note that order matters - see the note in onPause(), the reverse applies here.
					session.Resume();
				}

				// the app may crash here because of a race condition if you've not YET accepted camera 
				// permissions. just accept the permissions, and then when the app crashes, restart it,
				// and it should be fine. 
				surfaceView.OnResume();
			}
			else
			{
				ActivityCompat.RequestPermissions(this, new string[] { Android.Manifest.Permission.Camera }, 0);
			}
		}

		protected override void OnPause()
		{
			base.OnPause();

			if (session != null)
			{
				// Note that the order matters - GLSurfaceView is paused first so that it does not try
				// to query the session. If Session is paused before GLSurfaceView, GLSurfaceView may
				// still call session.update() and get a SessionPausedException.

				surfaceView.OnPause();
				session.Pause();
			}
		}

		protected override void OnDestroy()
		{
			if (session != null)
			{
				session.Close();
				session = null;
			}

			base.OnDestroy();
		}

		#endregion

		#region HELPERS

		private void showLoadingMessage()
		{
			this.RunOnUiThread(() =>
			{
				var mLoadingMessageSnackbar = Snackbar.Make(FindViewById(Android.Resource.Id.Content),
					"Searching for surfaces...", Snackbar.LengthIndefinite);
				mLoadingMessageSnackbar.View.SetBackgroundColor(Android.Graphics.Color.DarkGray);
				mLoadingMessageSnackbar.Show();
			});
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		#endregion

		#region SURFACEVUEW RENDERER

		public void OnDrawFrame(IGL10 gl)
		{
			// Clear screen to notify driver it should not load any pixels from previous frame.
			GLES20.GlClear(GLES20.GlColorBufferBit | GLES20.GlDepthBufferBit);

			if (session == null)
			{
				return;
			}

			try
			{
				session.SetCameraTextureName(backgroundRenderer.getTextureId());

				// Obtain the current frame from ARSession. When the configuration is set to
				// UpdateMode.BLOCKING (it is by default), this will throttle the rendering to the
				// camera framerate.
				Frame frame = session.Update();
				Camera camera = frame.Camera;

				// Get projection matrix.
				float[] projectionMatrix = new float[16];
				camera.GetProjectionMatrix(projectionMatrix, 0, 0.1f, 100.0f);

				// Get camera matrix and draw.
				float[] viewMatrix = new float[16];
				camera.GetViewMatrix(viewMatrix, 0);

				// Compute lighting from average intensity of the image.
				// The first three components are color scaling factors.
				// The last one is the average pixel intensity in gamma space.
				float[] colorCorrectionRgba = new float[4];
				frame.LightEstimate.GetColorCorrection(colorCorrectionRgba, 0);

				// If frame is ready, render camera preview image to the GL surface.
				backgroundRenderer.draw(frame);

				// Keep the screen unlocked while tracking, but allow it to lock when tracking stops.
				trackingStateHelper.updateKeepScreenOnFlag(camera.TrackingState);

				// ARCore's face detection works best on upright faces, relative to gravity.
				// If the device cannot determine a screen side aligned with gravity, face
				// detection may not work optimally.
				Collection<AugmentedFace> faces = (Collection<AugmentedFace>)session.GetAllTrackables(Java.Lang.Class.FromType(typeof(AugmentedFace)));

				foreach (AugmentedFace face in faces)
				{
					if (face.TrackingState != TrackingState.Tracking)
					{
						break;
					}

					float scaleFactor = 1.0f;

					// Face objects use transparency so they must be rendered back to front without depth write.
					GLES20.GlDepthMask(false);

					// Each face's region poses, mesh vertices, and mesh normals are updated every frame.

					// 1. Render the face mesh first, behind any 3D objects attached to the face regions.
					float[] modelMatrix = new float[16];
					face.CenterPose.ToMatrix(modelMatrix, 0);
					augmentedFaceRenderer.draw(
						projectionMatrix, viewMatrix, modelMatrix, colorCorrectionRgba, face);

					// 2. Next, render the 3D objects attached to the forehead.
					face.GetRegionPose(RegionType.ForeheadRight).ToMatrix(rightEarMatrix, 0);
					rightEarObject.updateModelMatrix(rightEarMatrix, scaleFactor);
					rightEarObject.draw(viewMatrix, projectionMatrix, colorCorrectionRgba, DEFAULT_COLOR);

					face.GetRegionPose(RegionType.ForeheadLeft).ToMatrix(leftEarMatrix, 0);
					leftEarObject.updateModelMatrix(leftEarMatrix, scaleFactor);
					leftEarObject.draw(viewMatrix, projectionMatrix, colorCorrectionRgba, DEFAULT_COLOR);

					// 3. Render the nose last so that it is not occluded by face mesh or by 3D objects attached
					// to the forehead regions.
					face.GetRegionPose(RegionType.NoseTip).ToMatrix(noseMatrix, 0);
					noseObject.updateModelMatrix(noseMatrix, scaleFactor);
					noseObject.draw(viewMatrix, projectionMatrix, colorCorrectionRgba, DEFAULT_COLOR);
				}

			}
			catch (Exception t)
			{
				// Avoid crashing the application due to unhandled exceptions.
				Log.Error(TAG, "Exception on the OpenGL thread", t);
			}
			finally
			{
				GLES20.GlDepthMask(true);
			}
		}

		public void OnSurfaceChanged(IGL10 gl, int width, int height)
		{
			GLES20.GlViewport(0, 0, width, height);
		}

		public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
		{
			GLES20.GlClearColor(0.1f, 0.1f, 0.1f, 1.0f);

			// Prepare the rendering objects. This involves reading shaders, so may throw an IOException.
			try
			{
				// Create the texture and pass it to ARCore session to be filled during update().
				backgroundRenderer.createOnGlThread(/*context=*/ this);
				augmentedFaceRenderer.createOnGlThread(this, "models/freckles.png");
				augmentedFaceRenderer.setMaterialProperties(0.0f, 1.0f, 0.1f, 6.0f);
				noseObject.createOnGlThread(/*context=*/ this, "models/nose.obj", "models/nose_fur.png");
				noseObject.setMaterialProperties(0.0f, 1.0f, 0.1f, 6.0f);
				noseObject.setBlendMode(ObjectRenderer.BlendMode.AlphaBlending);
				rightEarObject.createOnGlThread(this, "models/forehead_right.obj", "models/ear_fur.png");
				rightEarObject.setMaterialProperties(0.0f, 1.0f, 0.1f, 6.0f);
				rightEarObject.setBlendMode(ObjectRenderer.BlendMode.AlphaBlending);
				leftEarObject.createOnGlThread(this, "models/forehead_left.obj", "models/ear_fur.png");
				leftEarObject.setMaterialProperties(0.0f, 1.0f, 0.1f, 6.0f);
				leftEarObject.setBlendMode(ObjectRenderer.BlendMode.AlphaBlending);
			}
			catch (Exception e)
			{
				Android.Util.Log.Error(TAG, "Failed to read an asset file", e);
			}
		}

		#endregion
	}
}
	