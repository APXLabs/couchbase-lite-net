using Android.App;
using TinyIoC;

namespace SeekableEncryptedVideo
{
    public class App : Application
    {
        public override void OnCreate()
        {
            base.OnCreate();
            var container = TinyIoCContainer.Current;

            container.Register<Storage, Storage>();

            var storage = container.Resolve<Storage>();
            var video = Assets.Open("dizzy.mp4");
            storage.InsertVideo(video);
        }
    }
}