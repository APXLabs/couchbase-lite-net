//
// tlsasync.cs: Multi-sessions TLS/SSL Test Program with async streams
//	based on tlstest.cs and tlsmulti.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

using Mono.Security.Protocol.Tls;

public class State {

	static ArrayList handleList = new ArrayList ();

	private int id;
	private HttpWebRequest request;
	private ManualResetEvent handle;
	private Stream stream;
	private byte[] buffer;
	private MemoryStream memory;

	public State (int id, HttpWebRequest req)
	{
		this.id = id;
		request = req;
		handle = new ManualResetEvent (false);
		handleList.Add (handle);
	}

	public int Id {
		get { return id; }
	}

	public HttpWebRequest Request {
		get { return request; }
	}

	public Stream Stream {
		get { return stream; }
		set { stream = value; }
	}

	public byte[] Buffer {
		get {
			if (buffer == null)
				buffer = new byte [256]; // really small on purpose
			return buffer;
		}
	}

	public Stream Memory {
		get {
			if (memory == null)
				memory = new MemoryStream ();
			return memory;
		}
	}

	public void Complete ()
	{
		handle.Set ();
	}

	static public void WaitAll ()
	{
		if (handleList.Count > 0) {
			WaitHandle[] handles = (WaitHandle[]) handleList.ToArray (typeof (WaitHandle));
			WaitHandle.WaitAll (handles);
			handleList.Clear ();
		}
	}
}

public class MultiTest {

	static bool alone;

	public static void Main (string[] args) 
	{
		if (args.Length == 0) {
			Console.WriteLine ("usage: mono tlsaync.exe url1 [url ...]");
			return;
		} else if (args.Length > 64) {
			Console.WriteLine ("WaitHandle has a limit of 64 handles so you cannot process {0} URLs.", args.Length);
			return;
		}

		alone = (args.Length == 1);
		ServicePointManager.CertificatePolicy = new TestCertificatePolicy ();

		int id = 1;
		foreach (string url in args) {
			Console.WriteLine ("GET #{0} at {1}", id, url);
			HttpWebRequest wreq = (HttpWebRequest) WebRequest.Create (url);
			State s = new State (id++, wreq);
			wreq.BeginGetResponse (new AsyncCallback (ResponseCallback), s);
		}

		State.WaitAll ();
	}

	private static void ResponseCallback (IAsyncResult result)
	{
		State state = ((State) result.AsyncState);
		HttpWebResponse response = (HttpWebResponse) state.Request.EndGetResponse (result);
		state.Stream = response.GetResponseStream ();
		state.Stream.BeginRead (state.Buffer, 0, state.Buffer.Length, new AsyncCallback (StreamCallBack), state);
	}

	private static void StreamCallBack (IAsyncResult result)
	{
		State state = ((State) result.AsyncState);
		int length = state.Stream.EndRead (result);
		if (length > 0) {
			state.Memory.Write (state.Buffer, 0, length);
			state.Stream.BeginRead (state.Buffer, 0, state.Buffer.Length, new AsyncCallback (StreamCallBack), state);
		} else {
			state.Stream.Close ();
			if (alone) {
				state.Memory.Position = 0;
				StreamReader sr = new StreamReader (state.Memory, Encoding.UTF8);
				Console.WriteLine (sr.ReadToEnd ());
			}
			Console.WriteLine ("END #{0}", state.Id);
			state.Complete ();
		}
	}

	public class TestCertificatePolicy : ICertificatePolicy {

		public bool CheckValidationResult (ServicePoint sp, X509Certificate certificate, WebRequest request, int error)
		{
			// whatever the reason we do not stop the SSL connection
			return true;
		}
	}
}
