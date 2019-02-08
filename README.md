![LEAN Batch Launcher](https://user-images.githubusercontent.com/4928988/51481977-1fa4d080-1d8d-11e9-85f2-9ad344d95737.PNG)

*LEAN Batch Launcher* is an unofficial, alternative launcher for [QuantConnect's Lean Engine](https://github.com/quantconnect/lean) enabling batching/looping of algorithms in parallel with different start/end dates, securities, Alphas, data resolutions and (most importantly) parameters.

## Getting Started

### Prerequisites

Prerequisites are the same as for Lean.

The software has only been tested on Windows but may work fine on Linux and Mac.

### Installing

1. Make sure Lean builds and runs properly.
1. Clone the repository into a directory of choice. Ideally, this is not a folder inside the Lean folder.
1. Open [`LeanBatchLauncher.sln`](LeanBatchLauncher.sln) in Visual Studio and add necessary references to your pre-existing Lean project.
   1. Trying to compile will tell you what references you need. Keep adding references in both the `Algorithm`, `Launcher` and `Instance` projects to Lean's DLL files until you can compile successfully. You will need (at least) `Common`, `Configuration`, `Lean.Engine`, `Logging`, `Messaging` and `Queues`. To build the sample algorithm, you will also need `Algorithm` and `Algorithm.Framework`. The existing references (included without a `HintPath`) should be a guide to what's needed.
1. Edit [`Instance/Program.cs`](Instance/Program.cs) lines 75 and 76 to set the correct path to and name of the algorithm to be batched. By default, the `BasicTemplateFrameworkAlgorithm` is referenced. Whatever DLL you choose here must be referenced in `Launcher` and thus compiled at runtime.
1. Ensure [`Launcher/data-start-date-by-symbol.json`](Launcher/data-start-date-by-symbol.json) is filled in appropriately. Each Symbol to be used in the Launcher must have its earliest start date specified.
1. Open [`Launcher/batch.config.json`](Launcher/batch.config.json) and follow the reference guide below to configure it properly.
1. See [`Algorithm/BasicTemplateFrameworkAlgorithm.cs`](Algorithm/BasicTemplateFrameworkAlgorithm.cs) for an example algorithm that makes use of some of the Launcher's functionality. For more examples, see *Usage in algorithm* sections below.
1. Build (`F6`) the `Launcher` project once if it's not already selected by default, to ensure files are copied appropriately.

Done! You can now run the `Launcher` as you please.

## Configuring a batch in `batch.config.json`

The various options are described below. Non-optional parameters are marked as such.

| Parameter | Description and usage |
|-------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **`LibraryPath`** | Path to the root folder of the Lean project, i.e. where the `Launcher` and `Data` folders reside. Non-optional.<br><br> Example: `"LibraryPath": "C:\\Users\\John\\Algorithm\\Lib\\Lean"` <br> <br> Remember to use double backslashes (`\\`) to separate folders. |
| **`ApiJobUserId`** | Your API job user ID. Same as in Lean's `config.json`. Non-optional.<br><br> Example: `"ApiJobUserId": "32476"` |
| **`ApiAccessToken`** | Your API access token. Same as in Lean's `config.json`. Non-optional.<br><br> Example: `"ApiAccessToken": "O8dLVxwKhXpl4JiIfHWP25eIkgs8LY3r"` |
| **`ParallelProcesses`** | How many processes your computer can handle in parallel. Must be less than or equal to the number of virtual CPU cores available. Non-optional.<br><br> Example: `"ParallelProcesses": 7` |
| **`StartDate`** | The starting date of the first instance, expressed as "dd mmm yyyy" (or any other format that `DateTime` can parse). Non-optional.<br><br> Example: `"StartDate": "01 Jan 2018"` <br><br> Usage in algorithm: `SetStartDate( DateTime.Parse( Config.Get( "LBL-start-date" ) ) );` |
| **`Duration`** | The length of each backtest (in months). Only integers allowed. Non-optional.<br><br> Example: `"Duration": 12` |
| **`AlphaModelNames`** | An array of strings corresponding to the `Name` property of each Alpha you intend to run. Each Alpha you specify here must already have been created in your algorithm's main file. <br><br> Example: ``` "AlphaModelNames": [ "Alpha1(A=3,B=2)", "Alpha1(A=1,B=4)", "Alpha2(D=6)" ] ``` <br><br> Usage in algorithm: the Alphas intended for use must first be initialised in an `IAlphaModel[]` array. Then the Alpha can be selected by invoking `SetAlpha( arrayOfAlphas.Where( x => x.GetModelName() == Config.Get( "LBL-alpha-model-name" ) ).Single() );` |
| **`MinuteResolutions`** | An array of integers corresponding to different minute resolutions to pass to the algorithm, one at a time. <br><br> Example: ``` "MinuteResolutions": [    30,    60,    120 ] ``` <br><br> Usage in algorithm: `int minuteResolution = Config.GetInt( "LBL-minute-resolution" );`. You could then pass this into e.g. a consolidator. |
| **`Symbols`** | An array of symbol strings to pass to the algorithm, one at a time. <br><br> Example: ``` "Symbols": [ "EURUSD", "CORNUSD", "USDHKD" ] ``` <br><br> Usage in algorithm: `SetUniverseSelection( new ManualUniverseSelectionModel( QuantConnect.Symbol.Create( Config.Get( "LBL-symbol" ), SecurityType.Equity, Market.USA ) ) );` |
| **`Parameters`** | Allows you to pass in either a *step* or *factor* range: <br><br> **Step range:** Will step through a parameter from `Start` to `End` in fixed increments of `Step`. Example: ``` "Parameters": { "LBL-band-width": { "Start": 2, "End": 5, "Step": 0.5 } } ``` <br> *Will yield `LBL-band-width` of 2, 2.5, 3, 3.5, 4, 4.5 and 5.* <br> <br> Usage in algorithm: `var bandWidth = Config.GetDouble( "band-width" );`. <br> <br> **Factor range:** Will step through a parameter from `Start` to `End` in factors of `Factor`. Example: ``` "Parameters": { "LBL-lookback-period": { "Start": 4, "End": 64, "Factor": 2 } } ``` <br> *Will yield `LBL-lookback-period` of 4, 8, 16, 32 and 64.* <br> <br> Usage in algorithm: `var bandWidth = Config.GetInt( "lookback-period" );`. |

## Contributing

Feel free to submit Issues and/or Pull Requests with new functionality, fixes or other enhancements.

## Authors

* **Douglas Stridsberg**

See also the list of [contributors](https://github.com/Doggie52/Lean-Batch-Launcher/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [`LICENSE`](LICENSE) file for details

## Acknowledgments

* [The QuantConnect team](https://www.quantconnect.com/)
* [James](https://github.com/jameschch) and his brilliant [LeanOptimization library](https://github.com/jameschch/LeanOptimization) for his inspiration and words of wisdom
