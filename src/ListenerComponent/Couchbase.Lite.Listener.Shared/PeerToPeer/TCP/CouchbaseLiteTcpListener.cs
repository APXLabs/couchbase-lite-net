﻿//
//  CouchbaseLiteServiceListener.cs
//
//  Author:
//  	Jim Borden  <jim.borden@couchbase.com>
//
//  Copyright (c) 2015 Couchbase, Inc All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;

using Couchbase.Lite.Security;
using Couchbase.Lite.Util;
using WebSocketSharp.Net;

namespace Couchbase.Lite.Listener.Tcp
{
    [Flags]
    public enum CouchbaseLiteTcpOptions
    {
        Default = 0,
        AllowBasicAuth = 1 << 0,
        UseTLS = 1 << 1
    }

    /// <summary>
    /// An implementation of CouchbaseLiteServiceListener using TCP/IP
    /// </summary>
    public sealed class CouchbaseLiteTcpListener : CouchbaseLiteServiceListener
    {

        #region Constants

        private const int NONCE_TIMEOUT = 300;
        private const string TAG = "CouchbaseLiteTcpListener";

        #endregion

        #region Variables 

        private readonly HttpListener _listener;
        private Manager _manager;
        private bool _allowsBasicAuth;
        private bool _usesTLS;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager">The manager to use for opening DBs, etc</param>
        /// <param name="port">The port to listen on</param>
        /// <remarks>
        /// If running on Windows, check <a href="https://github.com/couchbase/couchbase-lite-net/wiki/Gotchas">
        /// This document</a>
        /// </remarks>
        public CouchbaseLiteTcpListener(Manager manager, ushort port, string realm = "Couchbase")
            : this(manager, port, CouchbaseLiteTcpOptions.Default, realm)
        {
            
        }

        public CouchbaseLiteTcpListener(Manager manager, ushort port, CouchbaseLiteTcpOptions options, string realm = "Couchbase")
            : this(manager, port, options, realm, null)
        {
            
        }

        public CouchbaseLiteTcpListener(Manager manager, ushort port, CouchbaseLiteTcpOptions options, X509Certificate2 sslCert)
            : this(manager, port, options, "Couchbase", sslCert)
        {

        }

        public CouchbaseLiteTcpListener(Manager manager, ushort port, CouchbaseLiteTcpOptions options, string realm, X509Certificate2 sslCert)
        {
            _manager = manager;
            _listener = new HttpListener();
            _usesTLS = options.HasFlag(CouchbaseLiteTcpOptions.UseTLS);
            string prefix = _usesTLS ? String.Format("https://*:{0}/", port) :
                String.Format("http://*:{0}/", port);
            _listener.Prefixes.Add(prefix);
            _listener.AuthenticationSchemeSelector = SelectAuthScheme;
            HttpListener.DefaultServerString = "Couchbase Lite " + Manager.VersionString;
            _listener.Realm = realm;
            _allowsBasicAuth = options.HasFlag(CouchbaseLiteTcpOptions.AllowBasicAuth);

            _listener.UserCredentialsFinder = GetCredential;
            if (options.HasFlag(CouchbaseLiteTcpOptions.UseTLS)) {
                _listener.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls;
                _listener.SslConfiguration.ClientCertificateRequired = false;
                if (sslCert == null) {
                    Log.To.Listener.I(TAG, "Generating X509 certificate for listener...");
                    sslCert = X509Manager.GenerateTransientCertificate("Couchbase-P2P");
                }

                Log.To.Listener.I(TAG, "Using X509 certificate {0} (issued by {1})",
                    sslCert.Subject, sslCert.Issuer);
                _listener.SslConfiguration.ServerCertificate = sslCert;
            }
        }

        #endregion

        #region Private Methods

        private AuthenticationSchemes SelectAuthScheme(HttpListenerRequest request)
        {
            if (request.Url.LocalPath == "/") {
                Log.To.Listener.V(TAG, "Disregarding authentication for root request");
                return AuthenticationSchemes.Anonymous;
            }

            if (RequiresAuth) {
                var schemes = AuthenticationSchemes.Digest;
                if (_allowsBasicAuth) {
                    schemes |= AuthenticationSchemes.Basic;
                }

                return schemes;
            }

            return AuthenticationSchemes.Anonymous;
        }

        private NetworkCredential GetCredential(IIdentity identity)
        {
            var password = default(string);
            if (!TryGetPassword(identity.Name, out password)) {
                return null;
            }

            Log.To.Listener.V(TAG, "Request from user {0}, so require password {1}",
                new SecureLogString(identity.Name, LogMessageSensitivity.PotentiallyInsecure),
                new SecureLogString(password, LogMessageSensitivity.Insecure));
            return new NetworkCredential(identity.Name, password);
        }

        //This gets called when the listener receives a request
        private void ProcessRequest (HttpListenerContext context)
        {
            var isLocal = System.Net.IPAddress.IsLoopback(context.Request.RemoteEndPoint.Address) ||
                          context.Request.LocalEndPoint == context.Request.RemoteEndPoint;

            Log.To.Listener.I(TAG, "Received new {0} {1} connection", 
                _usesTLS ? "secure" : "plain", isLocal ? "local" : "remote");
            var getContext = Task.Factory.FromAsync<HttpListenerContext>(_listener.BeginGetContext, _listener.EndGetContext, null);
            getContext.ContinueWith(t => ProcessRequest(t.Result));

            var internalContext = new CouchbaseListenerTcpContext(context.Request, context.Response, _manager);
            internalContext.IsLoopbackRequest = isLocal;
            var uriBuilder = new UriBuilder(_usesTLS ? "https" : "http", context.Request.LocalEndPoint.Address.ToString(),
                                 context.Request.LocalEndPoint.Port);
            uriBuilder.UserName = context.User != null && context.User.Identity != null ? context.User.Identity.Name : null;
            internalContext.Sender = uriBuilder.Uri;
            Log.To.Listener.D(TAG, "Sender set to {0}", internalContext.Sender);
            _router.HandleRequest(internalContext);
        }

        #endregion

        #region Overrides

        public override void Start()
        {
            if (_listener.IsListening) {
                return;
            }
                
            base.Start();
            _listener.Start();

            var getContext = Task.Factory.FromAsync<HttpListenerContext>(_listener.BeginGetContext, _listener.EndGetContext, null);
            getContext.ContinueWith(t => ProcessRequest(t.Result));
        }

        public override void Stop()
        {
            if (!_listener.IsListening) {
                return;
            }

            base.Stop();
            _listener.Stop();
        }

        public override void Abort()
        {
            if (!_listener.IsListening) {
                return;
            }

            base.Abort();
            _listener.Stop();
        }

        protected override void DisposeInternal()
        {
            ((IDisposable)_listener).Dispose();
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Format("CouchbaseLiteTcpListener[Prefixes={0}]", new LogJsonString(_listener.Prefixes));
        }

        #endregion

    }
}

