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

namespace SeekableEncryptedVideo
{
    [Activity(Label = "SeekableEncryptedVideo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private TinyIoCContainer _container;
        private Storage _storage;
        private TextView _textView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            _container = TinyIoCContainer.Current;
            _storage = _container.Resolve<Storage>();
            _textView = FindViewById<TextView>(Resource.Id.textView);
            Task.Run(() => DoTests());
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
    }
}

