using System;
using System.Collections.Generic;

namespace LeanBatchLauncher.Util
{
	/// <summary>
	/// Various extra enumerables.
	/// </summary>
	public static class EnumerableUtils
	{

		/// <summary>
		/// An enumerable range with custom start, stop and step.
		/// </summary>
		/// <param name="start">Starting point of range.</param>
		/// <param name="end">Last value in range (is not guaranteed to be hit).</param>
		/// <param name="step">Step size when getting next entry.</param>
		/// <returns></returns>
		public static IEnumerable<double> SteppedRange( double start, double end, double step = 1.0 )
		{

			// Check for zero step
			if ( step == 0 )
				throw new ArgumentException( "Parameter step cannot equal zero." );

			// Step through the range
			if ( start < end && step > 0 ) {
				for ( var x = start; x - end <= 0; x += step ) {
					yield return x;
				}
			} else {
				yield return start;
			}
		}

		/// <summary>
		/// Enumerable range of products of start and factor.
		/// x = start, start*factor^1, start*factor^2, ..., end
		/// </summary>
		/// <param name="start">Starting point of range.</param>
		/// <param name="end">Last value in range (is not guaranteed to be hit).</param>
		/// <param name="factor">The factor by which to multiply start each time.</param>
		/// <returns></returns>
		public static IEnumerable<double> ProductRange( double start, double end, double factor = 2 )
		{

			// Check for zero step
			if ( factor < 2 )
				throw new ArgumentException( "Parameter factor cannot be less than 2." );

			// Step through the range
			if ( start < end ) {
				double x = start;
				while ( x <= end ) {
					yield return x;
					x *= factor;
				}
			} else {
				yield return start;
			}
		}
	}
}
