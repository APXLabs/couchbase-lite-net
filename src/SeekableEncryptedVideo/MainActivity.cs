using System;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TinyIoC;
using Uri = Android.Net.Uri;

namespace SeekableEncryptedVideo
{
    [Activity(Label = "SeekableEncryptedVideo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, MediaController.IMediaPlayerControl
    {
        private TinyIoCContainer _container;
        private Storage _storage;
        private TextView _textView;
        private VideoView _videoView;
        private MediaController _mediacontroller;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _container = TinyIoCContainer.Current;
            _storage = _container.Resolve<Storage>();
            _textView = FindViewById<TextView>(Resource.Id.textView);
            _videoView = FindViewById < VideoView>(Resource.Id.videoView);
            _videoView.SetVideoURI(Uri.Parse("http://127.0.0.1:8001/fooo"));
            _videoView.Prepared += (sender, args) => _videoView.Start();

            _mediacontroller = new MediaController(this);
            _mediacontroller.SetAnchorView(_videoView);

            Task.Run(() => DoTests());
        }

        public override bool OnGenericMotionEvent(MotionEvent e)
        {
            _mediacontroller?.Show();
            return base.OnGenericMotionEvent(e);
        }

        private  void DoTests()
        {
            string theText = "This is some text";
            byte[] textBytes = Encoding.Default.GetBytes(theText);
            byte[] encryptedBytes = _storage.Encrypt(textBytes);
            byte[] decryptedBytes = _storage.Decrypt(encryptedBytes);
            string resultText = Encoding.Default.GetString(decryptedBytes);

            RunOnUiThread(() =>
            {
                _textView.Text += $"Original:\n{theText}\n";
                _textView.Text += $"After encrypt/decrypt:\n{resultText}\n";
            });
        }

        public bool CanPause()
        {
            return _videoView.CanPause();
        }

        public bool CanSeekBackward()
        {
            return _videoView.CanSeekBackward();
        }

        public bool CanSeekForward()
        {
            return _videoView.CanSeekForward();
        }

        public void Pause()
        {
            _videoView.Pause();
        }

        public void SeekTo(int pos)
        {
            _videoView.SeekTo(pos);
        }

        public void Start()
        {
            _videoView.Start();
        }

        public int AudioSessionId => _videoView.AudioSessionId;
        public int BufferPercentage => _videoView.BufferPercentage;
        public int CurrentPosition => _videoView.CurrentPosition;
        public int Duration => _videoView.Duration;
        public bool IsPlaying => _videoView.IsPlaying;
    }
}

