using Android.Content;
using Android.OS;
using Android.Renderscripts;
using Android.Views;
using Google.AR.Core;
using Google.AR.Sceneform.UX;
using System;
using Xamarin.Essentials;
using static Google.AR.Core.Config;

namespace XamarinARCore.Controller.ARCore
{
    public class FaceARFragment : ArFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            //PlaneDiscoveryController.Hide();
            //PlaneDiscoveryController.SetInstructionView(null);

            return view;
        }

        protected override Config GetSessionConfiguration(Session session)
        {
            var filter = new CameraConfigFilter(session).SetFacingDirection(CameraConfig.FacingDirection.Back);

            var configFilter = session.GetSupportedCameraConfigs(filter)[0];
            session.CameraConfig = configFilter;

            var config = new Config(session);
            //config.SetAugmentedFaceMode(AugmentedFaceMode.Mesh3d);

            session.Configure(config);

            //_actionComplete?.Invoke();
            return config;
        }
    }
}