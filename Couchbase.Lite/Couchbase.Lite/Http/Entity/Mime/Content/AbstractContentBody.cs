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

using System;
using System.IO;
using Org.Apache.Http.Entity.Mime.Content;
using Sharpen;

namespace Org.Apache.Http.Entity.Mime.Content
{
	/// <since>4.0</since>
	public abstract class AbstractContentBody : ContentBody
	{
		private readonly string mimeType;

		private readonly string mediaType;

		private readonly string subType;

		public AbstractContentBody(string mimeType) : base()
		{
			if (mimeType == null)
			{
				throw new ArgumentException("MIME type may not be null");
			}
			this.mimeType = mimeType;
			int i = mimeType.IndexOf('/');
			if (i != -1)
			{
				this.mediaType = Sharpen.Runtime.Substring(mimeType, 0, i);
				this.subType = Sharpen.Runtime.Substring(mimeType, i + 1);
			}
			else
			{
				this.mediaType = mimeType;
				this.subType = null;
			}
		}

		public virtual string GetMimeType()
		{
			return this.mimeType;
		}

		public virtual string GetMediaType()
		{
			return this.mediaType;
		}

		public virtual string GetSubType()
		{
			return this.subType;
		}

		public abstract string GetCharset();

		public abstract long GetContentLength();

		public abstract string GetTransferEncoding();

		public abstract string GetFilename();

		public abstract void WriteTo(OutputStream arg1);
	}
}
