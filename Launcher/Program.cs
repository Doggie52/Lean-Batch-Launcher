using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeanBatchLauncher.Util;

using Newtonsoft.Json;

using static CommandLineEncoder.Utils;
using static LeanBatchLauncher.Launcher.Configuration;

namespace LeanBatchLauncher.Launcher
{
	/// <summary>
	/// Contains functionality to run multiple instances of the QC engine in parallel.
	/// </summary>
	public class Program
	{

		/// <summary>
		/// Main function.
		/// </summary>
		/// <param name="args"></param>
		static void Main( string[] args )
		{

			// Start stopwatch
			var watch = Stopwatch.StartNew();

			// Read and parse the config file
			var userConfiguration = JsonConvert.DeserializeObject<Configuration>( File.ReadAllText( "batch.config.json" ) );

			// Read and parse the data start dates for each symbol
			var dataStartDateBySymbol = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>( File.ReadAllText( "data-start-date-by-symbol.json" ) );

			// If no alphas are provided, we must give one
			if ( userConfiguration.AlphaModelNames.Length == 0 )
				userConfiguration.AlphaModelNames = new string[] { "COMPOSITE" };

			// Generate ranges of parameters
			userConfiguration.PopulateParameterRanges();

			// If no parameters are provided, we must give one
			if ( userConfiguration.ParameterRanges.Count == 0 ) {
				userConfiguration.Parameters.Add( "NA", new Parameter() );
				userConfiguration.ParameterRanges.Add( "NA", EnumerableUtils.SteppedRange( 0, 0 ) );
			}

			// Generate list of dates
			userConfiguration.GenerateDates();

			// We need to first store each instance context in order to then run a `Parallel.ForEach()` on all of them
			var instanceContexts = new List<InstanceContext>();

			// Loop over start dates, alpha models + parameters, symbols, minute resolutions and eventually all parameter combinations
			foreach ( var startDate in userConfiguration.Dates ) {
				foreach ( string alphaModelName in userConfiguration.AlphaModelNames ) {
					foreach ( string symbol in userConfiguration.Symbols ) {
						foreach ( int minuteResolution in userConfiguration.MinuteResolutions ) {
							foreach ( var parameterCombination in userConfiguration.GenerateParameterCombinations( userConfiguration.ParameterRanges, new Dictionary<string, double>() ) ) {

								// Loop over each paramter provided and set it accordingly
								foreach ( var parameter in parameterCombination )
									userConfiguration.Parameters[parameter.Key].Current = parameter.Value;

								// Store the instance context to be looped over later
								instanceContexts.Add( new InstanceContext()
								{
									StartDate = startDate < dataStartDateBySymbol[symbol] ? dataStartDateBySymbol[symbol] : startDate,
									EndDate = startDate.AddMonths( userConfiguration.Duration ),    // We keep this at the same end date as all other symbols
									AlphaModelName = alphaModelName,
									Symbol = symbol,
									MinuteResolution = minuteResolution,
									ParametersSerialized = JsonConvert.SerializeObject( userConfiguration.Parameters )
								} );
							}
						}
					}
				}
			}

			Console.WriteLine( "Launching {0} threads at a time. Total of {1} backtests.", Math.Min( userConfiguration.ParallelProcesses, instanceContexts.Count ), instanceContexts.Count );

			// Shuffle instances
			var rng = new Random();
			instanceContexts = instanceContexts.OrderBy( r => rng.Next() ).ToList();

			// Run each instance in parallel
			Parallel.ForEach( instanceContexts, new ParallelOptions { MaxDegreeOfParallelism = userConfiguration.ParallelProcesses }, ( context ) =>
			{

				// Use ProcessStartInfo class.
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					CreateNoWindow = false,
					UseShellExecute = false,
					FileName = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Instance.exe" )
				};

				// Create arguments
				var builder = new StringBuilder();
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( userConfiguration.LibraryPath ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( userConfiguration.ApiJobUserId ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( userConfiguration.ApiAccessToken ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( context.StartDate.ToString( "yyyy-MM-dd" ) ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( context.EndDate.ToString( "yyyy-MM-dd" ) ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( context.AlphaModelName ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( context.Symbol ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( context.MinuteResolution.ToString() ) ) );
				builder.Append( String.Format( "\"{0}\" ", EncodeArgText( context.ParametersSerialized ) ) );

				// Append to StartInfo
				startInfo.Arguments = builder.ToString();

				// Start the process with the info we specified
				using ( Process exeProcess = Process.Start( startInfo ) ) {

					// Proceeed when process is finished
					exeProcess.WaitForExit();
				}
			} );

			// Exit and close
			watch.Stop();
			Console.WriteLine( string.Format( "Execution time: {0:0.0} minutes (total), {1:0.0} minute(s) (per backtest)", watch.ElapsedMilliseconds / 1000.0 / 60.0, watch.ElapsedMilliseconds / 1000.0 / 60.0 / instanceContexts.Count ) );
			Console.WriteLine( "Waiting for key press..." );
			Console.Read();
			Environment.Exit( 0 );

		}

		/// <summary>
		/// Contains one instance context to be passed into a backtest.
		/// </summary>
		public class InstanceContext
		{
			public DateTime StartDate;
			public DateTime EndDate;
			public string AlphaModelName;
			public string Symbol;
			public int MinuteResolution;
			public string ParametersSerialized;
		}
	}
}
