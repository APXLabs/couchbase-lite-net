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
using Sharpen;

namespace Couchbase.Lite
{
	/// <summary>A delegate that can be invoked to compile source code into a ReplicationFilter.
	/// 	</summary>
	/// <remarks>A delegate that can be invoked to compile source code into a ReplicationFilter.
	/// 	</remarks>
	public interface ReplicationFilterCompiler
	{
		/// <summary>Compile Filter Function</summary>
		/// <param name="source">The source code to compile into a ReplicationFilter.</param>
		/// <param name="language">The language of the source.</param>
		/// <returns>A compiled ReplicationFilter.</returns>
		ReplicationFilter CompileFilterFunction(string source, string language);
	}
}
