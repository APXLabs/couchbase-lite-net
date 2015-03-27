//
// ExtensionMethods.cs
//
// Author:
//     Zachary Gramana  <zack@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc
// Copyright (c) 2014 .NET Foundation
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
// Copyright (c) 2014 Couchbase, Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
// except in compliance with the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
// either express or implied. See the License for the specific language governing permissions
// and limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

using Newtonsoft.Json.Linq;

using Sharpen;
using System.Threading.Tasks;

namespace Couchbase.Lite
{
    internal delegate bool TryParseDelegate<T>(string s, out T value);

    internal static class ExtensionMethods
    {
        public static bool TryGetValue<T>(this IDictionary<string, object> dic, string key, out T value)
        {
            value = default(T);
            object obj;
            if (!dic.TryGetValue(key, out obj)) {
                return false;
            }

            if (!(obj is T)) {
                return false;
            }

            value = (T)obj;
            return true;
        }

        public static object JsonQuery(this NameValueCollection collection, string key)
        {
            string value = collection.Get(key);
            if (value == null) {
                return null;
            }

            return Manager.GetObjectMapper().ReadValue<object>(value);
        }

        public static T Get<T>(this NameValueCollection collection, string key, TryParseDelegate<T> parser, T defaultVal)
        {
            string value = collection.Get(key);
            if (value == null) {
                return defaultVal;
            }

            T retVal;
            if (!parser(value, out retVal)) {
                return defaultVal;
            }

            return retVal;
        }

        public static T GetCast<T>(this IDictionary<string, object> collection, string key)
        {
            return collection.GetCast(key, default(T));
        }

        public static T GetCast<T>(this IDictionary<string, object> collection, string key, T defaultVal)
        {
            object value = collection.Get(key);
            if (value == null) {
                return defaultVal;
            }
                
            if (!(value is T)) {
                return defaultVal;
            }

            return (T)value;
        }

        public static IEnumerable<T> AsSafeEnumerable<T>(this IEnumerable<T> source)
        {
            var e = ((IEnumerable)source).GetEnumerator();
            using (e as IDisposable)
            {
                while (e.MoveNext())
                {
                    yield return (T)e.Current;
                }
            }
        }

        internal static IDictionary<TKey,TValue> AsDictionary<TKey, TValue>(this object attachmentProps)
        {
            if (attachmentProps == null)
                return null;
            return attachmentProps is JObject
                ? ((JObject)attachmentProps).ToObject<IDictionary<TKey, TValue>>()
                    : (IDictionary<TKey, TValue>)attachmentProps;
        }

        internal static IList<TValue> AsList<TValue>(this object value)
        {
            if (value == null)
                return null;
            return value is JArray
                ? ((JArray)value).ToObject<IList<TValue>>()
                    : (IList<TValue>)value;
        }

        public static IEnumerable ToEnumerable(this IEnumerator enumerator)
        {
            while(enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }

        public static String Fmt(this String str, params Object[] vals)
        {
            return String.Format(str, vals);
        }

        public static Byte[] ReadAllBytes(this Stream stream)
        {
            var chunkBuffer = new byte[Attachment.DefaultStreamChunkSize];
            // We know we'll be reading at least 1 chunk, so pre-allocate now to avoid an immediate resize.
            var blob = new List<Byte> (Attachment.DefaultStreamChunkSize);

            int bytesRead;
            do {
                chunkBuffer.Initialize ();
                // Resets all values back to zero.
                bytesRead = stream.Read (chunkBuffer, blob.Count, Attachment.DefaultStreamChunkSize);
                blob.AddRange (chunkBuffer.Take (bytesRead));
            }
            while (bytesRead < stream.Length);

            return blob.ToArray();
        }

        public static StatusCode GetStatusCode(this HttpStatusCode code)
        {
            var validVals = Enum.GetValues(typeof(StatusCode));
            foreach (StatusCode validVal in validVals)
            {
                if ((Int32)code == (Int32)validVal)
                {
                    return validVal;
                }
            }

            return StatusCode.Unknown;
        }

        public static AuthenticationHeaderValue GetAuthenticationHeader(this Uri uri, string scheme)
        {
            Debug.Assert(uri != null);

            var unescapedUserInfo = uri.UserEscaped
                ? Uri.UnescapeDataString(uri.UserInfo)
                : uri.UserInfo;

            var param = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(unescapedUserInfo));

            return new AuthenticationHeaderValue(scheme, param);
        }

        public static AuthenticationHeaderValue AsAuthenticationHeader(this string userinfo, string scheme)
        {
            Debug.Assert(userinfo != null);

            var param = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userinfo));

            return new AuthenticationHeaderValue(scheme, param);
        }

        public static string ToQueryString(this IDictionary<string, object> parameters)
        {
            var maps = parameters.Select(kvp =>
            {
                var key = Uri.EscapeUriString(kvp.Key);

                string value;
                if (kvp.Value is string) 
                {
                    value = Uri.EscapeUriString(kvp.Value as String);
                }
                else if (kvp.Value is ICollection)
                {
                    value = Uri.EscapeUriString(Manager.GetObjectMapper().WriteValueAsString(kvp.Value));
                }
                else
                {
                    value = Uri.EscapeUriString(kvp.Value.ToString());
                }

                return String.Format("{0}={1}", key, value);
            });

            return String.Join("&", maps.ToArray());
        }

        #if NET_3_5

        public static IEnumerable<FileInfo> EnumerateFiles(this DirectoryInfo info, string searchPattern, SearchOption option) {
            foreach (var file in info.GetFiles(searchPattern, option)) {
                yield return file;
            }
        }

        public static Task<HttpListenerContext> GetContextAsync(this HttpListener listener)
        {
            return Task.Factory.FromAsync<HttpListenerContext>(listener.BeginGetContext, listener.EndGetContext, null);
        }

        #endif
    }
}
