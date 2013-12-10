/**
 * Couchbase Lite for .NET
 *
 * Original iOS version by Jens Alfke
 * Android Port by Marty Schoch, Traun Leyden
 * C# Port by Zack Gramana
 *
 * Copyright (c) 2012, 2013 Couchbase, Inc. All rights reserved.
 * Portions (c) 2013 Xamarin, Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the
 * License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language governing permissions
 * and limitations under the License.
 */

using System.Text;
using Org.Apache.Http.Entity.Mime;
using Sharpen;

namespace Org.Apache.Http.Entity.Mime
{
	/// <since>4.0</since>
	public sealed class MIME
	{
		public const string ContentType = "Content-Type";

		public const string ContentTransferEnc = "Content-Transfer-Encoding";

		public const string ContentDisposition = "Content-Disposition";

		public const string Enc8bit = "8bit";

		public const string EncBinary = "binary";

		/// <summary>The default character set to be used, i.e.</summary>
		/// <remarks>The default character set to be used, i.e. "US-ASCII"</remarks>
		public static readonly Encoding DefaultCharset = Sharpen.Extensions.GetEncoding("US-ASCII"
			);
	}
}
