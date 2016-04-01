using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Util;
using SeekableEncryptedVideo;

namespace CblCryptoTestApp
{

	/// <summary>
	/// Data representation of an HTTP range header
	/// </summary>
	public struct RangeHeader
	{
		/// <summary>
		/// Gets or sets the initial range position
		/// </summary>
		public long? From { get; private set; }

		/// <summary>
		/// Gets or sets the final range position
		/// </summary>
		public long? To { get; private set; }

		/// <summary>
		/// Parses header
		/// </summary>
		/// <param name="headerValue">Value of the Range header</param>
		/// <returns>Range object from header</returns>
		public static RangeHeader Parse(string headerValue)
		{
			RangeHeader range = new RangeHeader();
			long from;
			long to;
			// Range is in the form of {units}={first}-{last}. First and last can be empty
			string[] values = headerValue.Split(new[] { '=', '-' });
			if (long.TryParse(values[1], out from))
				range.From = from;
			if (values.Length > 2 && long.TryParse(values[2], out to))
				range.To = to;
			return range;
		}
	}


	/// <summary>
	/// See also: https://gist.github.com/aksakalli/9191056
	/// </summary>
	public class MediaHttpServer
	{
		public const string TAG = "MediaHttpServer";

		private HttpListener _listener;
		private bool _continueListening = true;
		private int _port = 8001;
	    private Storage _storage;

		/// <summary>
		/// Construct server with given port.
		/// </summary>
		/// <param name="path">Directory path to serve.</param>
		/// <param name="port">Port of the server.</param>
		public MediaHttpServer(Storage storage)
		{
			_continueListening = true;
			_storage = storage;
			
		}

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <returns></returns>
	    public void Start()
	    {
	        Task.Run(() => ListenServer());
        }


		private async void ListenServer()
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
			_listener.Start();

			while (_continueListening)
			{
				HttpListenerContext context = await _listener.GetContextAsync();
				Task.Factory.StartNew(HandleRequestTask, context);
			}
		}

		private void HandleRequestTask(object context)
		{
			try
			{
				HandleRequest(context as HttpListenerContext);
			}
			catch (Exception e)
			{
				Android.Util.Log.Error(TAG, string.Format("Exception while handling request: {0}", e));
			}
		}

		private void HandleRequest(HttpListenerContext httpContext)
		{
			HttpListenerResponse response = httpContext.Response;
			HttpListenerRequest request = httpContext.Request;
			string filepath = request.RawUrl;

			// using RawUrl because Url will decode the uri and encoded puncutation such as %2F turn back into /



			Stream fileStream = _storage.GetVideo();

			// Capture range
			string headerValue = request.Headers["Range"];
			if (string.IsNullOrEmpty(headerValue))
			{
				HandleFullRequest(response, fileStream);
			}
			else
			{
				HandleRangeRequest(response, fileStream, RangeHeader.Parse(headerValue));
			}
		}

		private void HandleFullRequest(HttpListenerResponse response, Stream inputStream)
		{
			Log.Debug(TAG, "HandleFullRequest");

			response.StatusCode = 200;
			response.AddHeader("Content-Length", inputStream.Length.ToString());

			// Decrypt stream
			try
			{

				inputStream.CopyTo(response.OutputStream);

				response.OutputStream.Close();
			}
			catch (Exception e)
			{
				Android.Util.Log.Error(TAG, string.Format("Exception while copying data {0}", e));
			}
		}

		private void HandleRangeRequest(HttpListenerResponse response, Stream inputStream, RangeHeader range)
		{
			Log.Debug(TAG, "HandleRangeRequest");
			if (range.From == null)
				throw new ArgumentNullException("range", "range.from must not be null");

			long offset = range.From.Value;
			if (offset > inputStream.Length)
			{
				response.StatusCode = 416;
				response.Close();
				return;
			}

			response.StatusCode = 206;
			response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", offset, inputStream.Length - 1, inputStream.Length));
			response.AddHeader("Content-Length", (inputStream.Length - offset).ToString());


			// Decrypt stream
			try
			{

				byte[] emptyBuffer = new byte[range.To.Value - range.From.Value];
				inputStream.Position = range.From.Value;

				inputStream.Read(emptyBuffer, 0, emptyBuffer.Length);


				response.OutputStream.Close();
			}
			catch (Exception e)
			{
				Android.Util.Log.Error(TAG, string.Format("Exception while copying data {0}", e));
			}

		}
	}
}