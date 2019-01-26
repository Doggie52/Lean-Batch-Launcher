using System;

using QuantConnect;
using QuantConnect.Algorithm.Framework;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Orders;

namespace Algorithm
{
	/// <summary>
	/// Basic template framework algorithm uses framework components to define the algorithm.
	/// </summary>
	/// <meta name="tag" content="using data" />
	/// <meta name="tag" content="using quantconnect" />
	/// <meta name="tag" content="trading and orders" />
	public class BasicTemplateFrameworkAlgorithm : QCAlgorithmFramework
	{
		/// <summary>
		/// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
		/// </summary>
		public override void Initialize()
		{
			// Set requested data resolution
			UniverseSettings.Resolution = Resolution.Minute;

			SetStartDate( 2013, 10, 07 );  //Set Start Date
			SetEndDate( 2013, 10, 11 );    //Set End Date
			SetCash( 100000 );             //Set Strategy Cash

			// Find more symbols here: http://quantconnect.com/data
			// Forex, CFD, Equities Resolutions: Tick, Second, Minute, Hour, Daily.
			// Futures Resolution: Tick, Second, Minute
			// Options Resolution: Minute Only.

			// set algorithm framework models
			SetUniverseSelection( new ManualUniverseSelectionModel( QuantConnect.Symbol.Create( "SPY", SecurityType.Equity, Market.USA ) ) );
			SetAlpha( new ConstantAlphaModel( InsightType.Price, InsightDirection.Up, TimeSpan.FromMinutes( 20 ), 0.025, null ) );
			SetPortfolioConstruction( new EqualWeightingPortfolioConstructionModel() );
			SetExecution( new ImmediateExecutionModel() );
			SetRiskManagement( new MaximumDrawdownPercentPerSecurity( 0.01m ) );
		}

		public override void OnOrderEvent( OrderEvent orderEvent )
		{
			if ( orderEvent.Status.IsFill() ) {
				Debug( $"Purchased Stock: {orderEvent.Symbol}" );
			}
		}
	}
}