using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.Hardware.Display;
using Google.AR.Core;
using Android.Views;
using Java.Lang;

namespace XamarinARCore.ARLibrary.Helpers
{
    public class DisplayRotationHelper : Java.Lang.Object, DisplayManager.IDisplayListener
    {
        private bool viewportChanged;
        private int viewportWidth;
        private int viewportHeight;
        private Display display;
        private DisplayManager displayManager;
        private CameraManager cameraManager;

        /**
		 * Constructs the DisplayRotationHelper but does not register the listener yet.
		 *
		 * @param context the Android {@link Context}.
		 */
        public DisplayRotationHelper(Context context)
        {
            displayManager = (DisplayManager)context.GetSystemService(Context.DisplayService);
            cameraManager = (CameraManager)context.GetSystemService(Context.CameraService);
            Android.Views.IWindowManager windowManager = (context as Activity).WindowManager; //(Android.Views.IWindowManager)context.GetSystemService(Context.WindowService);
            display = windowManager.DefaultDisplay;
        }

        /** Registers the display listener. Should be called from {@link Activity#onResume()}. */
        public void onResume()
        {
            displayManager.RegisterDisplayListener(this, null);
        }

        /** Unregisters the display listener. Should be called from {@link Activity#onPause()}. */
        public void onPause()
        {
            displayManager.UnregisterDisplayListener(this);
        }

        /**
         * Records a change in surface dimensions. This will be later used by {@link
         * #updateSessionIfNeeded(Session)}. Should be called from {@link
         * android.opengl.GLSurfaceView.Renderer
         * #onSurfaceChanged(javax.microedition.khronos.opengles.GL10, int, int)}.
         *
         * @param width the updated width of the surface.
         * @param height the updated height of the surface.
         */
        public void onSurfaceChanged(int width, int height)
        {
            viewportWidth = width;
            viewportHeight = height;
            viewportChanged = true;
        }

        /**
         * Updates the session display geometry if a change was posted either by {@link
         * #onSurfaceChanged(int, int)} call or by {@link #onDisplayChanged(int)} system callback. This
         * function should be called explicitly before each call to {@link Session#update()}. This
         * function will also clear the 'pending update' (viewportChanged) flag.
         *
         * @param session the {@link Session} object to update if display geometry changed.
         */
        public void updateSessionIfNeeded(Session session)
        {
            if (viewportChanged)
            {
                int displayRotation = ((int)display.Rotation);
                session.SetDisplayGeometry(displayRotation, viewportWidth, viewportHeight);
                viewportChanged = false;
            }
        }

        /**
         *  Returns the aspect ratio of the GL surface viewport while accounting for the display rotation
         *  relative to the device camera sensor orientation.
         */
        public float getCameraSensorRelativeViewportAspectRatio(string cameraId)
        {
            float aspectRatio;
            int cameraSensorToDisplayRotation = getCameraSensorToDisplayRotation(cameraId);
            switch (cameraSensorToDisplayRotation)
            {
                case 90:
                case 270:
                    aspectRatio = (float)viewportHeight / (float)viewportWidth;
                    break;
                case 0:
                case 180:
                    aspectRatio = (float)viewportWidth / (float)viewportHeight;
                    break;
                default:
                    throw new RuntimeException("Unhandled rotation: " + cameraSensorToDisplayRotation);
            }
            return aspectRatio;
        }

        /**
         * Returns the rotation of the back-facing camera with respect to the display. The value is one of
         * 0, 90, 180, 270.
         */
        public int getCameraSensorToDisplayRotation(string cameraId)
        {
            CameraCharacteristics characteristics;
            try
            {
                characteristics = cameraManager.GetCameraCharacteristics(cameraId);
            }
            catch (CameraAccessException e)
            {
                throw new RuntimeException("Unable to determine display orientation", e);
            }

            // Camera sensor orientation.
            int sensorOrientation = (int)characteristics.Get(CameraCharacteristics.SensorOrientation);

            // Current display orientation.
            int displayOrientation = toDegrees((int)display.Rotation);

            // Make sure we return 0, 90, 180, or 270 degrees.
            return (sensorOrientation - displayOrientation + 360) % 360;
        }


        private int toDegrees(int rotation)
        {
            switch (rotation)
            {
                case 0:
                    return 0;
                case 1:
                    return 90;
                case 2:
                    return 180;
                case 3:
                    return 270;
                default:
                    throw new RuntimeException("Unknown rotation " + rotation);
            }
        }



        public void OnDisplayAdded(int displayId) { }


        public void OnDisplayRemoved(int displayId) { }


        public void OnDisplayChanged(int displayId)
        {
            viewportChanged = true;
        }
    }
}