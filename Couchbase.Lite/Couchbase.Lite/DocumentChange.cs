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
using Com.Couchbase.Lite.Internal;
using Sharpen;

namespace Com.Couchbase.Lite
{
	public class DocumentChange
	{
		internal DocumentChange(RevisionInternal revisionInternal, bool isCurrentRevision
			, bool isConflict, Uri sourceUrl)
		{
			this.revisionInternal = revisionInternal;
			this.isCurrentRevision = isCurrentRevision;
			this.isConflict = isConflict;
			this.sourceUrl = sourceUrl;
		}

		private RevisionInternal revisionInternal;

		private bool isCurrentRevision;

		private bool isConflict;

		private Uri sourceUrl;

		public virtual string GetDocumentId()
		{
			return revisionInternal.GetDocId();
		}

		public virtual string GetRevisionId()
		{
			return revisionInternal.GetRevId();
		}

		public virtual bool IsCurrentRevision()
		{
			return isCurrentRevision;
		}

		public virtual bool IsConflict()
		{
			return isConflict;
		}

		public virtual Uri GetSourceUrl()
		{
			return sourceUrl;
		}

		[InterfaceAudience.Private]
		public virtual RevisionInternal GetRevisionInternal()
		{
			return revisionInternal;
		}

		public static Com.Couchbase.Lite.DocumentChange TempFactory(RevisionInternal revisionInternal
			, Uri sourceUrl)
		{
			bool isCurrentRevFixMe = false;
			// TODO: fix this to have a real value
			bool isConflictRevFixMe = false;
			// TODO: fix this to have a real value
			Com.Couchbase.Lite.DocumentChange change = new Com.Couchbase.Lite.DocumentChange(
				revisionInternal, isCurrentRevFixMe, isConflictRevFixMe, sourceUrl);
			return change;
		}
	}
}
