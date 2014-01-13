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
using System.Collections.Generic;
using Couchbase.Lite;
using Couchbase.Lite.Support;
using Couchbase.Lite.Util;
using Sharpen;

namespace Couchbase.Lite.Support
{
	/// <summary>
	/// Utility that queues up objects until the queue fills up or a time interval elapses,
	/// then passes all the objects at once to a client-supplied processor block.
	/// </summary>
	/// <remarks>
	/// Utility that queues up objects until the queue fills up or a time interval elapses,
	/// then passes all the objects at once to a client-supplied processor block.
	/// </remarks>
	public class Batcher<T>
	{
		private ScheduledExecutorService workExecutor;

		private ScheduledFuture<object> flushFuture;

		private int capacity;

		private int delay;

		private IList<T> inbox;

		private BatchProcessor<T> processor;

		private bool shuttingDown = false;

		private sealed class _Runnable_26 : Runnable
		{
			public _Runnable_26(Batcher<T> _enclosing)
			{
				this._enclosing = _enclosing;
			}

			public void Run()
			{
				try
				{
					this._enclosing.ProcessNow();
				}
				catch (Exception e)
				{
					// we don't want this to crash the batcher
					Log.E(Database.Tag, "BatchProcessor throw exception", e);
				}
			}

			private readonly Batcher<T> _enclosing;
		}

		private Runnable processNowRunnable;

		public Batcher(ScheduledExecutorService workExecutor, int capacity, int delay, BatchProcessor
			<T> processor)
		{
			processNowRunnable = new _Runnable_26(this);
			this.workExecutor = workExecutor;
			this.capacity = capacity;
			this.delay = delay;
			this.processor = processor;
		}

		public virtual void ProcessNow()
		{
			IList<T> toProcess = null;
			lock (this)
			{
				if (inbox == null || inbox.Count == 0)
				{
					return;
				}
				toProcess = inbox;
				inbox = null;
				flushFuture = null;
			}
			if (toProcess != null)
			{
				processor.Process(toProcess);
			}
		}

		public virtual void QueueObject(T @object)
		{
			lock (this)
			{
				if (inbox != null && inbox.Count >= capacity)
				{
					Flush();
				}
				if (inbox == null)
				{
					inbox = new AList<T>();
					if (workExecutor != null)
					{
						flushFuture = workExecutor.Schedule(processNowRunnable, delay, TimeUnit.Milliseconds
							);
					}
				}
				inbox.AddItem(@object);
			}
		}

		public virtual void Flush()
		{
			lock (this)
			{
				if (inbox != null)
				{
					bool didcancel = false;
					if (flushFuture != null)
					{
						didcancel = flushFuture.Cancel(false);
					}
					//assume if we didn't cancel it was because it was already running
					if (didcancel)
					{
						ProcessNow();
					}
					else
					{
						Log.V(Database.Tag, "skipping process now because didcancel false");
					}
				}
			}
		}

		public virtual int Count()
		{
			lock (this)
			{
				if (inbox == null)
				{
					return 0;
				}
				return inbox.Count;
			}
		}

		public virtual void Close()
		{
		}
	}
}
