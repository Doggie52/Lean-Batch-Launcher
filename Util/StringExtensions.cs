using System;
using System.IO;

namespace LeanBatchLauncher.Util
{
	/// <summary>
	/// Contains various extensions to the String class.
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Writes to a file in a thread-safe manner using a Mutex lock.
		/// Overwrites files by default.
		/// </summary>
		/// <param name="output">Input string to write to the file.</param>
		/// <param name="filePath">Path of file to write to.</param>
		/// <param name="overwrite">Whether to overwrite pre-existing files.</param>
		public static void SafelyWriteToFile( this string output, string filePath, bool overwrite = true )
		{

			// Unique id for global mutex
			string fileName = Path.GetFileNameWithoutExtension( filePath );

			// Write to file thread safely
			ThreadSafe.Execute( fileName, () =>
			{
				if ( overwrite )
					File.WriteAllText( filePath, output );
				else
					File.AppendAllText( filePath, output );
			} );

		}

		/// <summary>
		/// Returns the type of number (int/decimal) contained in a string.
		/// </summary>
		/// <param name="maybeNumber">Input string maybe containing a number.</param>
		/// <returns>Type of number (int/decimal) contained. If no number found, type of string.</returns>
		public static Type GetTypeIfIntOrDecimal( this string maybeNumber )
		{

			// Try int or decimal
			bool isInt = int.TryParse( maybeNumber, out int n );
			bool isDecimal = decimal.TryParse( maybeNumber, out decimal d );

			// Check int first as it's more picky
			if ( isInt )
				return typeof( int );
			else if ( isDecimal )
				return typeof( decimal );

			return typeof( string );
		}
	}
}