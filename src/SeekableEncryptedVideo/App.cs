using System;
using Android.App;
using Android.Runtime;
using CblCryptoTestApp;
using TinyIoC;

namespace SeekableEncryptedVideo
{
    [Application]
    public class App : Application
    {
        public App(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer) { }

        public override void OnCreate()
        {
            base.OnCreate();

            var container = TinyIoCContainer.Current;
            container.Register<Storage, Storage>().AsSingleton();
            container.Register<MediaHttpServer, MediaHttpServer>().AsSingleton(); ;

            var storage = container.Resolve<Storage>();
            var video = Assets.Open("dizzy.mp4");
            bool done = storage.InsertVideo(video).Result;

            var server = container.Resolve <MediaHttpServer>();
            server.Start();
        }
    }
}