/**
 * Couchbase Lite for .NET
 *
 * Original iOS version by Jens Alfke
 * Android Port by Marty Schoch, Traun Leyden
 * C# Port by Zack Gramana
 *
 * Copyright (c) 2012, 2013, 2014 Couchbase, Inc. All rights reserved.
 * Portions (c) 2013, 2014 Xamarin, Inc. All rights reserved.
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
using Sharpen;
using Sharpen.Reflect;

namespace Couchbase.Lite.Util
{
	/// <summary>
	/// This class is used to pass full generics type information, and
	/// avoid problems with type erasure (that basically removes most
	/// usable type references from runtime Class objects).
	/// </summary>
	/// <remarks>
	/// This class is used to pass full generics type information, and
	/// avoid problems with type erasure (that basically removes most
	/// usable type references from runtime Class objects).
	/// It is based on ideas from
	/// &lt;a href="http://gafter.blogspot.com/2006/12/super-type-tokens.html"
	/// &gt;http://gafter.blogspot.com/2006/12/super-type-tokens.html</a>,
	/// Additional idea (from a suggestion made in comments of the article)
	/// is to require bogus implementation of <code>Comparable</code>
	/// (any such generic interface would do, as long as it forces a method
	/// with generic type to be implemented).
	/// to ensure that a Type argument is indeed given.
	/// <p>
	/// Usage is by sub-classing: here is one way to instantiate reference
	/// to generic type <code>List&lt;Integer&gt;</code>:
	/// <pre>
	/// TypeReference ref = new TypeReference&lt;List&lt;Integer&gt;&gt;() { };
	/// </pre>
	/// which can be passed to methods that accept TypeReference.
	/// </remarks>
	public abstract class TypeReference<T> : Comparable<Couchbase.Lite.Util.TypeReference
		<T>>
	{
		internal readonly Type _type;

		protected internal TypeReference()
		{
			Type superClass = GetType().GetGenericSuperclass();
			if (superClass is Type)
			{
				// sanity check, should never happen
				throw new ArgumentException("Internal error: TypeReference constructed without actual type information"
					);
			}
			_type = ((ParameterizedType)superClass).GetActualTypeArguments()[0];
		}

		public virtual Type GetType()
		{
			return _type;
		}

		/// <summary>
		/// The only reason we define this method (and require implementation
		/// of <code>Comparable</code>) is to prevent constructing a
		/// reference without type information.
		/// </summary>
		/// <remarks>
		/// The only reason we define this method (and require implementation
		/// of <code>Comparable</code>) is to prevent constructing a
		/// reference without type information.
		/// </remarks>
		public virtual int CompareTo(Couchbase.Lite.Util.TypeReference<T> o)
		{
			// just need an implementation, not a good one... hence:
			return 0;
		}
	}
}
