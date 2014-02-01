using System;
using System.Threading;

namespace ErsatzAcumen
{
	/// <summary>
	/// Utilities for locking class types.
	/// Potential-deadlock checking is provided
	/// in debug mode, while release code does not.
	/// </summary>
	public class ThreadSafe
	{
#if DEBUG

		static TimeSpan	maxWaited;
		static long		TimesWaited;

		static ThreadSafe()
		{
			maxWaited = TimeSpan.Zero;
			TimesWaited = 0;
		}

		static public void Enter(object obj)
		{
			try
			{
				bool isOk = true;
				for(TimeSpan Wait = new TimeSpan(0,0,1);
					isOk == true;
					Wait.Add(TimeSpan.FromSeconds(10)) )
				{
					if(Monitor.TryEnter(obj, Wait) == true)
					{//lock acquired
						return;
					}
					else
					{//lock not acquired

						Interlocked.Increment(ref TimesWaited);
						lock(maxWaited)
						{
							if(maxWaited < Wait)
							{
								maxWaited = Wait;
								Console.Write("Potential deadlock, maxWaited is now {0} seconds. Waiting more...",maxWaited.Seconds);
							}
						}
					}
				}
			}
			catch
			{
				throw;
			}

		}

		static public void Exit(object obj)
		{
			Monitor.Exit(obj);
		}

#else
		static public void Enter(object obj)
		{
			try
			{
				Monitor.Enter(obj);
			}
			catch
			{
				throw;
			}
		}

		static public void Exit(object obj)
		{
			try
			{
				Monitor.Exit(obj);
			}
			catch
			{
				throw;
			}
		}

#endif

	}
}

