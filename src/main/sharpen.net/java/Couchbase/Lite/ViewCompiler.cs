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

using Couchbase.Lite;
using Couchbase.Lite.Internal;
using Sharpen;

namespace Couchbase.Lite
{
	/// <summary>An external object that knows how to map source code of some sort into executable functions.
	/// 	</summary>
	/// <remarks>An external object that knows how to map source code of some sort into executable functions.
	/// 	</remarks>
	public interface ViewCompiler
	{
		/// <summary>Compiles source code into a MapDelegate.</summary>
		/// <remarks>Compiles source code into a MapDelegate.</remarks>
		/// <param name="source">The source code to compile into a Mapper.</param>
		/// <param name="language">The language of the source.</param>
		/// <returns>A compiled Mapper.</returns>
		[InterfaceAudience.Public]
		Mapper CompileMap(string source, string language);

		/// <summary>Compiles source code into a ReduceDelegate.</summary>
		/// <remarks>Compiles source code into a ReduceDelegate.</remarks>
		/// <param name="source">The source code to compile into a Reducer.</param>
		/// <param name="language">The language of the source.</param>
		/// <returns>A compiled Reducer</returns>
		[InterfaceAudience.Public]
		Reducer CompileReduce(string source, string language);
	}
}
