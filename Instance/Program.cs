using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;

using LeanBatchLauncher.Util;

using Newtonsoft.Json;

using QuantConnect;
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Logging;
using QuantConnect.Util;

using static CommandLineEncoder.Utils;
using static LeanBatchLauncher.Launcher.Configuration;

namespace LeanBatchLauncher.Instance
{

	/// <summary>
	/// One instance of an Algorithm run.
	/// </summary>
	public class Program
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		public static void Main( string[] args )
		{

			// Run the instance, decoding the arguments one-by-one
			Run(
				DecodeArgText( args[0] ),
				DecodeArgText( args[1] ),
				DecodeArgText( args[2] ),
				DateTime.Parse( DecodeArgText( args[3] ) ),
				DateTime.Parse( DecodeArgText( args[4] ) ),
				DecodeArgText( args[5] ),
				DecodeArgText( args[6] ),
				Int32.Parse( DecodeArgText( args[7] ) ),
				DecodeArgText( args[8] )
				);
		}

		/// <summary>
		/// Runs an instance of a backtest.
		/// </summary>
		/// <param name="libraryPath"></param>
		/// <param name="apiJobUserId"></param>
		/// <param name="apiAccessToken"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <param name="alphaModelName"></param>
		/// <param name="symbol"></param>
		/// <param name="minuteResolution"></param>
		/// <param name="parametersSerialized"></param>
		public static void Run( string libraryPath, string apiJobUserId, string apiAccessToken, DateTime startDate, DateTime endDate, string alphaModelName, string symbol, int minuteResolution, string parametersSerialized )
		{

			// Initiate a thread safe operation, as it seems we need to do all of the below in a thread safe manner
			ThreadSafe.Execute( "config", () =>
			{

				// Copy the config file thread safely
				File.Copy( Path.Combine( libraryPath, "Launcher/config.json" ), Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "config.json" ), true );

				Config.SetConfigurationFile( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "config.json" ) );
				Config.Reset();

				// Configure path to and name of algorithm
				Config.Set( "algorithm-type-name", "BasicTemplateFrameworkAlgorithm" );
				Config.Set( "algorithm-location", "Algorithm.dll" );

				// Set some values local to this Launcher
				Config.Set( "environment", "backtesting" );
				Config.Set( "data-folder", Path.Combine( libraryPath, "Data/" ) );
				Config.Set( "job-queue-handler", "LeanBatchLauncher.Launcher.Queue" );
				Config.Set( "data-provider", "QuantConnect.Lean.Engine.DataFeeds.ApiDataProvider" );
				Config.Set( "job-user-id", apiJobUserId );
				Config.Set( "api-access-token", apiAccessToken );

				// Set start and end dates
				Config.Set( "LBL-start-date", startDate.ToString() );
				Config.Set( "LBL-end-date", endDate.ToString() );

				// Parse alpha model and parameters
				Config.Set( "LBL-alpha-model-name", alphaModelName );

				// Set symbol
				Config.Set( "LBL-symbol", symbol );

				// Set minute resolution
				Config.Set( "LBL-minute-resolution", minuteResolution.ToString() );

				// Deserialize parameters
				var parameters = JsonConvert.DeserializeObject<Dictionary<string, Parameter>>( parametersSerialized );

				// Save parameters
				foreach ( KeyValuePair<string, Parameter> entry in parameters ) {
					Config.Set( entry.Key, entry.Value.Current.ToString() );
				}

			}
			);

			Log.DebuggingEnabled = false;

			// We only need console output - no logging
			using ( Log.LogHandler = new ConsoleLogHandler() ) {

				Log.Trace( "Engine.Main(): LEAN ALGORITHMIC TRADING ENGINE v" + Globals.Version + " Mode: *CUSTOM BATCH* (" + ( Environment.Is64BitProcess ? "64" : "32" ) + "bit)" );
				Log.Trace( "Engine.Main(): Started " + DateTime.Now.ToShortTimeString() );

				// Import external libraries specific to physical server location (cloud/local)
				LeanEngineSystemHandlers leanEngineSystemHandlers;
				try {
					leanEngineSystemHandlers = LeanEngineSystemHandlers.FromConfiguration( Composer.Instance );
				} catch ( CompositionException compositionException ) {
					Log.Error( "Engine.Main(): Failed to load library: " + compositionException );
					throw;
				}

				// Setup packeting, queue and controls system: These don't do much locally.
				leanEngineSystemHandlers.Initialize();

				//-> Pull job from QuantConnect job queue, or, pull local build:
				var job = leanEngineSystemHandlers.JobQueue.NextJob( out string assemblyPath );

				if ( job == null ) {
					throw new Exception( "Engine.Main(): Job was null." );
				}

				LeanEngineAlgorithmHandlers leanEngineAlgorithmHandlers;
				try {
					leanEngineAlgorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration( Composer.Instance );
				} catch ( CompositionException compositionException ) {
					Log.Error( "Engine.Main(): Failed to load library: " + compositionException );
					throw;
				}

				try {
					var algorithmManager = new AlgorithmManager( false, job );

					leanEngineSystemHandlers.LeanManager.Initialize( leanEngineSystemHandlers, leanEngineAlgorithmHandlers, job, algorithmManager );

					var engine = new Engine( leanEngineSystemHandlers, leanEngineAlgorithmHandlers, false );
					engine.Run( job, algorithmManager, assemblyPath, WorkerThread.Instance );

				} finally {
					// Delete the message from the job queue:
					leanEngineSystemHandlers.JobQueue.AcknowledgeJob( job );
					Log.Trace( "Engine.Main(): Packet removed from queue: " + job.AlgorithmId );

					// Clean up resources
					leanEngineSystemHandlers.Dispose();
					leanEngineAlgorithmHandlers.Dispose();
					Log.LogHandler.Dispose();

					Log.Trace( "Program.Main(): Exiting Lean..." );
				}
			}
		}
	}
}