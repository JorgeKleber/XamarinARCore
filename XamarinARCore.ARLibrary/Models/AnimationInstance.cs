using Com.Google.Android.Filament.Gltfio;
using Google.AR.Sceneform.UX;


namespace XamarinARCore.ARLibrary.Models
{
    public class AnimationInstance
    {
        Animator animator;
        long startTime;
        float duration;
        int index;

        public AnimationInstance(Animator animator, int index, long startTime)
        {
            this.animator = animator;
            this.index = index;
            this.duration = animator.GetAnimationDuration(index);
            this.startTime = startTime;
        }
    }
}