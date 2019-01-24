using System;
using System.IO;
using System.Threading;

namespace LeanBatchLauncher.Util
{
	/// <summary>
	/// Collection of threadsafe methods.
	/// </summary>
	public static class ThreadSafe
	{
		/// <summary>
		/// Runs a delegate action thread safely using a Mutex lock.
		/// </summary>
		/// <remarks>Inspired by https://stackoverflow.com/a/229567 .</remarks>
		/// <param name="identifier">Name of the Mutex lock.</param>
		/// <param name="func">The delegate action to run.</param>
		public static void Execute( string identifier, Action func )
		{

			// Unique id for global mutex - Global prefix means it is global to the machine
			// We use an identifier to ensure the mutex is only held for a particular purpose
			string mutexId = string.Format( "Global\\{{{0}}}", identifier );

			// We create/query the Mutex
			using ( var mutex = new Mutex( false, mutexId ) ) {

				var hasHandle = false;

				try {

					try {

						// We wait for lock to release
						hasHandle = mutex.WaitOne( Timeout.Infinite, false );

					} catch ( AbandonedMutexException ) {

						// The mutex was abandoned in another process,
						// it will still get acquired
						hasHandle = true;
					}

					// Execute action with retry loop
					for ( int i = 0; i < int.MaxValue; ++i ) {
						try {
							func();
							break; // When done we can break loop
						} catch ( IOException e ) when ( i < int.MaxValue ) {

							Thread.Sleep( 100 );
						}
					}					

				} finally {

					// If we have the Mutex, we release it
					if ( hasHandle )
						mutex.ReleaseMutex();
				}
			}
		}
	}
}
