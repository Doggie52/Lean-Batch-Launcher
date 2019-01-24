using System;
using System.Collections.Generic;
using System.Linq;

using LeanBatchLauncher.Util;

namespace LeanBatchLauncher.Launcher
{
	/// <summary>
	/// Contains configuration we want to pass each batched instance of the backtest.
	/// </summary>
	[Serializable]
	public class Configuration : MarshalByRefObject
	{

		/// <summary>
		/// Initialises a new instance of the <see cref="Configuration"/> class.
		/// </summary>
		public Configuration()
		{
			ParameterRanges = new Dictionary<string, IEnumerable<double>>();
			Dates = new List<DateTime>();
		}

		/// <summary>
		/// Generates and populates Ranges from all provided Parameters.
		/// </summary>
		/// <returns>A copy of itself to facilitate chaining.</returns>
		public Configuration PopulateParameterRanges()
		{
			// Loop through all Parameters, adding either a normal stepped range or a product range
			foreach ( KeyValuePair<string, Parameter> entry in Parameters ) {
				if ( entry.Value.Step != null )
					ParameterRanges.Add( entry.Key, EnumerableUtils.SteppedRange( entry.Value.Start, entry.Value.End, (double)entry.Value.Step ) );
				else if ( entry.Value.Factor != null )
					ParameterRanges.Add( entry.Key, EnumerableUtils.ProductRange( entry.Value.Start, entry.Value.End, (double)entry.Value.Factor ) );
			}

			return this;
		}

		/// <summary>
		/// Generates all possible combinations of all parameter ranges provided, recursively.
		/// <param name="args">The parameter ranges provided as input.</param>
		/// <param name="feedDown">The dictionary of parameters (string+double) to feed down the recursion.</param>
		/// <param name="nextKvp">The next key-value-pair to be added to the feed down dictioanry - defaults to an empty one.</param>
		/// <returns>Yields one combination of the input parameters provided.</returns>
		public IEnumerable<Dictionary<string, double>> GenerateParameterCombinations(
			Dictionary<string, IEnumerable<double>> args,
			Dictionary<string, double> feedDown,
			KeyValuePair<string, double> nextKvp = new KeyValuePair<string, double>()
			)
		{

			// Check for null inputs
			if ( args == null || args.Count == 0 )
				yield break;

			// Add the given key-value pair to the feed down dictionary (if it's not the first run)
			if ( nextKvp.Key != null ) {

				// If we don't have the key already, we add it
				if ( !feedDown.ContainsKey( nextKvp.Key ) )
					feedDown.Add( nextKvp.Key, 0.0 );

				// Update the value of this key
				feedDown[nextKvp.Key] = nextKvp.Value;
			}

			// Check for last argument - if it is, yield return a complete feed
			if ( args.Count == 1 ) {

				// Get the name of the parameter we've got left
				string lastKey = args.Keys.LastOrDefault();

				// Loop over all the doubles in the last parameter range
				foreach ( double lastValue in args.Values.LastOrDefault() ) {

					var outDict = new Dictionary<string, double>();

					// First we yield the existing values in the feed-down
					foreach ( var entry in feedDown )
						outDict.Add( entry.Key, entry.Value );

					// Then we add the last new parameter
					outDict.Add( lastKey, lastValue );

					// Then we yield the last new parameter
					yield return outDict;
				}

			} else {

				// Get the (random) key we will iterate over next
				string newKey = args.Keys.FirstOrDefault();

				// Iterate over the enumerable in this key
				foreach ( double newValue in args[newKey] ) {

					// Create a new list of arguments with the key in question removed
					var newArgs = new Dictionary<string, IEnumerable<double>>();
					foreach ( var kvp in args )
						newArgs.Add( kvp.Key, kvp.Value );
					newArgs.Remove( newKey );

					// Generate new kvp to send into the recursion
					var newKvp = new KeyValuePair<string, double>( newKey, newValue );

					// Recurse
					foreach ( var item in GenerateParameterCombinations( newArgs, feedDown, newKvp ) )
						yield return item;
				}
			}
		}

		/// <summary>
		/// Generate Dates from the provided StartDate and Duration.
		/// </summary>
		/// <returns>A copy of itself to faciliate chaining.</returns>
		public Configuration GenerateDates()
		{

			DateTime date = DateTime.Parse( StartDate );

			// Loop through the range {StartDate, StartDate + Duration, StartDate + 2*Duration, ..., Today}
			// Ensure we get at least the start date in there even if duration is huge
			do {
				Dates.Add( date );
				date = date.AddMonths( Duration );
			} while ( date.AddMonths( Duration ) <= DateTime.Now );

			return this;
		}

		/// <summary>
		/// Gets or sets the path to the folder containing the QuantConnect library.
		/// </summary>
		public string LibraryPath
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets Job User ID for the QC data API.
		/// </summary>
		public string ApiJobUserId
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets Access Token for the QC data API.
		/// </summary>
		public string ApiAccessToken
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets number of backtests to run in parallel.
		/// </summary>
		public int ParallelProcesses
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets the starting date of the backtest batch.
		/// </summary>
		public string StartDate
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets number of months to run each backtest for.
		/// </summary>
		/// <remarks>The range {StartDate, Today} will be chopped up in x slices, each of length Duration.</remarks>
		public int Duration
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets the alpha models (as `Names`) to run the algorithm over.
		/// </summary>
		public string[] AlphaModelNames
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets minute resolutions to run the algorithm over.
		/// </summary>
		public int[] MinuteResolutions
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets list of symbols to run the backtests on.
		/// </summary>
		public string[] Symbols
		{
			get; set;
		}

		/// <summary>
		/// Gets or sets any custom parameters intended to be looped over.
		/// </summary>
		public Dictionary<string, Parameter> Parameters
		{
			get; set;
		}

		/// <summary>
		/// Represents a single Parameter intended to be looped over.
		/// </summary>
		[Serializable]
		public class Parameter
		{
			public double Start;

			public double End;

			public double? Step;

			public double? Factor;

			private double _current = 0.0;

			public double Current
			{
				get {
					if ( _current > 0.0 )
						return _current;
					else
						return Start;
				}
				set {
					_current = value;
				}
			}
		}

		/// <summary>
		/// Gets a dictionary of all ParameterRanges generated, indexed by name of parameter.
		/// </summary>
		public Dictionary<string, IEnumerable<double>> ParameterRanges
		{
			get;
		}

		/// <summary>
		/// Gets a list of all starting dates to be used for backtests.
		/// </summary>
		public List<DateTime> Dates
		{
			get;
		}
	}
}
