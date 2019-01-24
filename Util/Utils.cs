using System;
using System.Text.RegularExpressions;

namespace CommandLineEncoder
{
	public class Utils
	{
		private const string EscapedSlashN = "[SlashN]";

		public static string EncodeArgText(string original)
		{
			var result = original;

			result = TryEncodeNewLine(result);

			result = TryEncodeSlashesFollowedByQuotes(result);

			result = TryEncodeQuotes(result);

			result = TryEncodeLastSlash(result);

			return result;
		}

		private static string TryEncodeNewLine(string original)
		{
			var result = original.Replace("\\n", EscapedSlashN);

			result = result.Replace(Environment.NewLine, "\\n");
			return result;
		}

		private static string TryEncodeSlashesFollowedByQuotes(string original)
		{
			var regexPattern = @"\\+""";

			string result = Regex.Replace(original, regexPattern,
				delegate(Match match)
				{
					string matchText = match.ToString();
					string justSlashes = matchText.Remove(matchText.Length - 1);
					return justSlashes + justSlashes + "\"";  //double up the slashes
				});

			return result;
		}

		private static string TryEncodeQuotes(string original)
		{
			var result = original.Replace("\"", "\"\"");
			return result;
		}

		private static string TryEncodeLastSlash(string original)
		{
			var regexPattern = @"\\+$";

			string result = Regex.Replace(original, regexPattern,
				delegate(Match match)
				{
					string matchText = match.ToString();
					return matchText + matchText;  //double up the slashes
				});

			return result;
		}

		public static string DecodeArgText(string original)
		{
			string decoded = original;

			decoded = TryDecodeNewLine(decoded);

			return decoded;
		}


		private static string TryDecodeNewLine(string original)
		{
			var result = original.Replace("\\n", Environment.NewLine);

			result = result.Replace(EscapedSlashN, "\\n");

			return result;
		}



		public static string EscapeBackSlashes(string text)
		{
			var regexPattern = "\\\\";

			var result = text;

			var regex = new Regex(regexPattern);

			var matches = regex.Matches(text);

			for (int i = matches.Count - 1; i >= 0; i--)
			{
				var match = matches[i];

				var index = match.Index + match.Length;

				if (index >= text.Length || text[index] == '\\')
					result = result.Insert(match.Index, "\\");
			}

			return result;
		}

	}
}
