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

using System.Collections.Generic;
using Sharpen;

namespace Couchbase.Lite.Support
{
	public class SequenceMap
	{
		private TreeSet<long> sequences;

		private long lastSequence;

		private IList<string> values;

		private long firstValueSequence;

		public SequenceMap()
		{
			sequences = new TreeSet<long>();
			values = new AList<string>(100);
			firstValueSequence = 1;
			lastSequence = 0;
		}

		public virtual long AddValue(string value)
		{
			lock (this)
			{
				sequences.AddItem(++lastSequence);
				values.AddItem(value);
				return lastSequence;
			}
		}

		public virtual void RemoveSequence(long sequence)
		{
			lock (this)
			{
				sequences.Remove(sequence);
			}
		}

		public virtual bool IsEmpty()
		{
			lock (this)
			{
				return sequences.IsEmpty();
			}
		}

		public virtual long GetCheckpointedSequence()
		{
			lock (this)
			{
				long sequence = lastSequence;
				if (!sequences.IsEmpty())
				{
					sequence = sequences.First() - 1;
				}
				if (sequence > firstValueSequence)
				{
					// Garbage-collect inaccessible values:
					int numToRemove = (int)(sequence - firstValueSequence);
					for (int i = 0; i < numToRemove; i++)
					{
						values.Remove(0);
					}
					firstValueSequence += numToRemove;
				}
				return sequence;
			}
		}

		public virtual string GetCheckpointedValue()
		{
			lock (this)
			{
				int index = (int)(GetCheckpointedSequence() - firstValueSequence);
				return (index >= 0) ? values[index] : null;
			}
		}
	}
}
