using System;
using System.Linq;

using QuantConnect;
using QuantConnect.Configuration;
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
			// Note: Config.GetInt( "LBL-minute-resolution" ) is available to use for consolidators

			// Set start and end date
			SetStartDate( DateTime.Parse( Config.Get( "LBL-start-date" ) ) );
			SetEndDate( DateTime.Parse( Config.Get( "LBL-end-date" ) ) );

			// Set universe
			SetUniverseSelection( new ManualUniverseSelectionModel( QuantConnect.Symbol.Create( Config.Get( "LBL-symbol" ), SecurityType.Forex, Market.Oanda ) ) );

			// Get alpha parameters
			int fastEma = Config.GetInt( "LBL-ema-fast" );
			int slowEma = Config.GetInt( "LBL-ema-slow" );

			// Initialise available alphas
			var availableAlphas = new IAlphaModel[]{
				new EmaCrossAlphaModel( fastEma, slowEma, Resolution.Minute )
			};

			// Set the alpha, for now we're assuming "ALL" has been passed
			if ( Config.Get( "LBL-alpha-model-name" ) == "ALL" )
				SetAlpha( new CompositeAlphaModel( availableAlphas ) );

			// Set remaining models
			SetPortfolioConstruction( new EqualWeightingPortfolioConstructionModel() );
			SetExecution( new ImmediateExecutionModel() );
			SetRiskManagement( new NullRiskManagementModel() );
		}

		public override void OnOrderEvent( OrderEvent orderEvent )
		{
			if ( orderEvent.Status.IsFill() ) {
				Debug( $"Purchased Stock: {orderEvent.Symbol}" );
			}
		}
	}
}