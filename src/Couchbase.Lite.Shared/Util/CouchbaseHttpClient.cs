using System.Net.Http;

namespace Couchbase.Lite.Shared.Util
{
    /// <summary>
    /// CouchbaseHttpClient is a http client that will prevent itself from being disposed.
    /// </summary>
    internal class CouchbaseHttpClient : HttpClient
    {
        public CouchbaseHttpClient(HttpMessageHandler handler) : base(handler, true)
        {
        }

        protected override void Dispose(bool disposing)
        {
            //Prevent the client from being disposed
        }
    }
}