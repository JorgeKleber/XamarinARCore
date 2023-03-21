using Android.App;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.App;
using Google.Android.Material.Snackbar;
using Google.AR.Core;
using Google.AR.Core.Exceptions;
using Javax.Microedition.Khronos.Opengles;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using XamarinARCore.Helpers;
using XamarinARCore.Rendering;
using static Google.AR.Core.AugmentedFace;
using Config = Google.AR.Core.Config;

namespace XamarinARCore
{
    [Activity(Label = "@string/app_name",
              Theme = "@style/AppTheme",
              MainLauncher = true,
              ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, GLSurfaceView.IRenderer
    {
        #region CONFIGURATIONS/INITIALIZING

        private static string TAG = typeof(MainActivity).Name;

        //renderizador. As renderizações são criadas usando este objeto e inicializado quando o surfaceview é criado.
        /// <summary>
        /// Renderizador - Um componente de UI para renderizar imagens na tela.
        /// </summary>
        private GLSurfaceView surfaceView;

        private bool userRequestedInstall = true;
        //responsável por iniciar a sessão.
        private Session session;
        //private SnackbarHelper messageSnackbarHelper = new SnackbarHelper();
        private DisplayRotationHelper displayRotationHelper;
        private TrackingStateHelper trackingStateHelper = new TrackingStateHelper(Android.App.Application.Context as Activity);

        //Instanciando os renderizadores.
        private readonly BackgroundRenderer backgroundRenderer = new BackgroundRenderer();
        private readonly AugmentedFaceRenderer augmentedFaceRenderer = new AugmentedFaceRenderer();
        private readonly ObjectRenderer noseObject = new ObjectRenderer();
        private readonly ObjectRenderer rightEarObject = new ObjectRenderer();
        private readonly ObjectRenderer leftEarObject = new ObjectRenderer();


        //Temporary matrix allocated here to reduce number of allocations for each frame.
        private readonly float[] noseMatrix = new float[16];
        private readonly float[] rightEarMatrix = new float[16];
        private readonly float[] leftEarMatrix = new float[16];
        private static readonly float[] DEFAULT_COLOR = new float[] { 0f, 0f, 0f, 0f };

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

            displayRotationHelper = new DisplayRotationHelper(Platform.CurrentActivity);
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await OnResumeAsync();
        }

        private async Task OnResumeAsync()
        {
            var arAvailability = ArCoreApk.Instance.CheckAvailability(this);

            if (arAvailability.IsUnsupported) //TODO: Create a unsuported view renderer
                return;

            // ARCore requires camera permission to operate.
            var permissionResult = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (permissionResult != PermissionStatus.Granted)
            {
                permissionResult = await Permissions.RequestAsync<Permissions.Camera>();

                if (permissionResult != PermissionStatus.Granted)
                    return;
            }

            try
            {
                if (session is null)
                {
                    var installResult = ArCoreApk.Instance.RequestInstall(Platform.CurrentActivity, userRequestedInstall);

                    if (installResult == ArCoreApk.InstallStatus.Installed)
                    {
                        // Success: Safe to create the AR session.
                        session = new Session(this);
                    }
                    else if (installResult == ArCoreApk.InstallStatus.InstallRequested)
                    {
                        // When this method returns `INSTALL_REQUESTED`:
                        // 1. ARCore pauses this activity.
                        // 2. ARCore prompts the user to install or update Google Play
                        //    Services for AR (market://details?id=com.google.ar.core).
                        // 3. ARCore downloads the latest device profile data.
                        // 4. ARCore resumes this activity. The next invocation of
                        //    requestInstall() will either return `INSTALLED` or throw an
                        //    exception if the installation or update did not succeed.
                        userRequestedInstall = false;
                        return;
                    }

                    // Set a camera configuration that usese the front-facing camera.
                    var filter = new CameraConfigFilter(session).SetFacingDirection(CameraConfig.FacingDirection.Front);

                    var cameraConfig = session.GetSupportedCameraConfigs(filter)[0];
                    session.CameraConfig = cameraConfig;

                    var config = new Config(session);
                    config.SetAugmentedFaceMode(Config.AugmentedFaceMode.Mesh3d);
                    session.Configure(config);
                }
            }
            catch (UnavailableUserDeclinedInstallationException ex)
            {
                //TODO: Feedback for user that dont want to install arcore
                return;
            }

            session.Resume();
            //if (View is GLSurfaceView surfaceView)
            //    surfaceView.OnResume();
            surfaceView.OnResume();

            displayRotationHelper.onResume();
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (session is null)
                return;

            session.Pause();

            //if (View is GLSurfaceView surfaceView)
            //    surfaceView.OnPause();
            surfaceView.OnPause();

            displayRotationHelper.onPause();
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

        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}

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

            // Notify ARCore session that the view size changed so that the perspective matrix and
            // the video background can be properly adjusted.
            displayRotationHelper.updateSessionIfNeeded(session);

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
                var faces = session.GetAllTrackables(Java.Lang.Class.FromType(typeof(AugmentedFace)));

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
                    augmentedFaceRenderer.draw(projectionMatrix, viewMatrix, modelMatrix, colorCorrectionRgba, face);

                    // 2. Next, render the 3D objects attached to the forehead.
                    face.GetRegionPose(RegionType.ForeheadRight).ToMatrix(rightEarMatrix, 0);
                    rightEarObject.UpdateModelMatrix(rightEarMatrix, scaleFactor);
                    rightEarObject.Draw(viewMatrix, projectionMatrix, colorCorrectionRgba, DEFAULT_COLOR);

                    face.GetRegionPose(RegionType.ForeheadLeft).ToMatrix(leftEarMatrix, 0);
                    leftEarObject.UpdateModelMatrix(leftEarMatrix, scaleFactor);
                    leftEarObject.Draw(viewMatrix, projectionMatrix, colorCorrectionRgba, DEFAULT_COLOR);

                    // 3. Render the nose last so that it is not occluded by face mesh or by 3D objects attached
                    // to the forehead regions.
                    face.GetRegionPose(RegionType.NoseTip).ToMatrix(noseMatrix, 0);
                    noseObject.UpdateModelMatrix(noseMatrix, scaleFactor);
                    noseObject.Draw(viewMatrix, projectionMatrix, colorCorrectionRgba, DEFAULT_COLOR);
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
            displayRotationHelper.onSurfaceChanged(width, height);
            GLES20.GlViewport(0, 0, width, height);
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            GLES20.GlClearColor(0.1f, 0.1f, 0.1f, 1.0f);

            // Prepare the rendering objects. This involves reading shaders, so may throw an IOException.
            try
            {
                // Create the texture and pass it to ARCore session to be filled during update().
                backgroundRenderer.createOnGlThread(this, -1);
                augmentedFaceRenderer.createOnGlThread(this, "models/freckles.png");
                augmentedFaceRenderer.setMaterialProperties(0.0f, 1.0f, 0.1f, 6.0f);

                TryCreateTexture(noseObject, "models/nose.obj", "models/nose_fur.png");
                TryCreateTexture(rightEarObject, "models/forehead_right.obj", "models/ear_fur.png");
                TryCreateTexture(leftEarObject, "models/forehead_left.obj", "models/ear_fur.png");

            }
            catch (Exception e)
            {
                Android.Util.Log.Error(TAG, "Failed to read an asset file", e);
            }
        }

        private void TryCreateTexture(ObjectRenderer renderer, string objectName, string assetTexture)
        {
            try
            {
                renderer.CreateOnGlThread(this, objectName, assetTexture);
                renderer.SetMaterialProperties(0.0f, 1.0f, 0.1f, 6.0f);
            }
            catch (Exception e)
            {
                Android.Util.Log.Error(TAG, "Failed to create texture", e);
                Android.Util.Log.Error(TAG, objectName);
            }
        }

        #endregion
    }
}