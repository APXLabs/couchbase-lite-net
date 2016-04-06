using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Lite.Shared.Util
{
    /// <summary>
    /// CouchbaseHttpClientHandler is a custom HttpClientHandler that reduces the leaked memory from the way the base class's mono implementation uses memorystreams.
    /// </summary>
    internal class CouchbaseHttpClientHandler : HttpClientHandler
    {
        private const string TAG = "CouchbaseLiteHttpClientHandler";
        bool _disposed;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().ToString());

            HttpWebResponse resp = null;
            HttpWebRequest req = (HttpWebRequest) WebRequest.Create(request.RequestUri);

            // Register to abort the web request upon cancellation of a request
            using (cancellationToken.Register(l => ((HttpWebRequest) l).Abort(), req))
            {
                req.Method = request.Method.Method;

                // Pass through cookies
                if (UseCookies) req.CookieContainer = CookieContainer;

                // Set use default credentials if set
                if (UseDefaultCredentials) req.UseDefaultCredentials = true;

                // Pass through headers
                if (request.Content != null)
                {
                    UpdateHeaders(req, request.Content.Headers);

                    using (Stream stream = await request.Content.ReadAsStreamAsync())
                    {
                        req.AllowWriteStreamBuffering = false;
                        req.ContentLength = stream.Length;

                        using (Stream reqStream = req.GetRequestStream())
                        {
                            stream.CopyTo(reqStream);
                        }
                    }
                }
                else
                {
                    UpdateHeaders(req, request.Headers);
                }

                try
                {
                    resp = (HttpWebResponse) req.GetResponse();
                }
                catch (WebException we)
                {
                    // Event though a WebException occurs, it still might be a response from the server
                    // We still want to use the response in that case.  If the request was cancelled, 
                    // the next block will handle through the cancellation token.  Otherwise, re-throw the exception

                    resp = we.Response as HttpWebResponse;
                    if (resp == null && we.Status != WebExceptionStatus.RequestCanceled) throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception when getting response! {0}", e);
                    throw;
                }
            }

            // Since we didn't throw the WebException if it was cancelled, this will handle that case
            if (cancellationToken.IsCancellationRequested)
            {
                var cancelled = new TaskCompletionSource<HttpResponseMessage>();
                cancelled.SetCanceled();
                return await cancelled.Task;
            }

            // The if statement above should prevent this from happening.  In case it doesn't, this
            // should at least provide a useful stack trace.
            if (resp == null)
            {
                throw new NullReferenceException(
                    "The HttpWebResponse was null.  The WebRequest was likely cancelled, but not the CancellationToken");
            }


            HttpResponseMessage responseMessage = CreateResponseMessage(request, resp, cancellationToken);
            return responseMessage;
        }


        private HttpResponseMessage CreateResponseMessage(HttpRequestMessage request, HttpWebResponse resp,
            CancellationToken cancellationToken)
        {
            StreamContent streamContent = new CouchbaseStreamContent(resp.GetResponseStream(), cancellationToken);

            HttpResponseMessage responseMessage = new HttpResponseMessage(resp.StatusCode)
            {
                Content = streamContent,
                RequestMessage = request
            };

            for (int i = 0; i < resp.Headers.Count; i++)
            {
                string key = resp.Headers.GetKey(i);
                string[] values = resp.Headers.GetValues(i);
                try
                {
                    switch (key)
                    {
                        /*
                         * Couchbase sync gateway provides a malformed ETag header. 
                         * It should be formatted with quote strings around it. For example...
                         * ETag: "3-b664c5c5df953d563767956c453751b2"
                         * However, it is currently formed as follows
                         * ETag: 3-b664c5c5df953d563767956c453751b2
                         */
                        case "ETag":
                            break;
                        case "Content-Length":
                        case "Content-Type":
                            responseMessage.Content.Headers.Add(key, values);
                            break;
                        default:
                            responseMessage.Headers.Add(key, values);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception when adding header \"{0}: {1}\" {2}", key, string.Join(", ", values), e);
                }
            }

            request.RequestUri = resp.ResponseUri;
            return responseMessage;
        }

        private static void UpdateHeaders(HttpWebRequest webRequest,
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                string headerValue = string.Join(", ", header.Value);
                if (WebHeaderCollection.IsRestricted(header.Key) == false)
                {
                    webRequest.Headers.Add(header.Key, headerValue);
                    continue;
                }

                switch (header.Key)
                {
                    case "Accept":
                        webRequest.Accept = headerValue;
                        break;
                    case "Content-Type":
                        webRequest.ContentType = headerValue;
                        break;
                    default:
                        Console.WriteLine("Encountered unhandled restricted header {0}", header.Key);
                        break;
                }
            }
        }

        #region IDisposable

        /// <summary>
        /// a finalizer is not necessary, as it is inherited from
        /// the base class
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }

    /// <summary>
    /// StreamContent in mono only holds onto a reference to the CancellationToken.  This 
    /// version uses it to dispose of the underlying stream
    /// </summary>
    internal class CouchbaseStreamContent : StreamContent
    {
        private CancellationTokenRegistration cancellationTokenRegistration;
        private bool _disposed;

        public CouchbaseStreamContent(Stream content, CancellationToken cancellationToken)
            : base(content)
        {
            cancellationTokenRegistration =
                cancellationToken.Register(stream => ((CouchbaseStreamContent) stream).Dispose(), this);
        }

        #region IDisposable

        /// <summary>
        /// a finalizer is not necessary, as it is inherited from
        /// the base class
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    cancellationTokenRegistration.Dispose();
                }

                // release any unmanaged objects
                // set object references to null

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}