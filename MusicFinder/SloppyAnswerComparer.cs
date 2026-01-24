namespace MusicFinder
{
	/// <summary>
	/// Methods to compare user answers with correct artist and track names, allowing for minor errors like typos, omitting "The " from the start, etc.
	/// </summary>
	public static partial class SloppyAnswerComparer
	{
		public static bool AreCloseEnough(string? answer, string? guess)
		{
			if (string.IsNullOrWhiteSpace(answer) || string.IsNullOrWhiteSpace(guess))
			{
				return false;
			}

			string cleanAnswer = NormalizeString(answer);
			string cleanGuess = NormalizeString(guess);

			cleanAnswer = NormalizeString(cleanAnswer);
			cleanGuess = NormalizeString(cleanGuess);

			if (cleanGuess.Length == 0)
			{
				return false;
			}

			if (cleanAnswer == cleanGuess)
			{
				return true;
			}

			// Allow for minor typos using Levenshtein distance
			int distance = LevenshteinDistance(cleanAnswer, cleanGuess);
			int maxAllowedDistance = Math.Max(1, cleanAnswer.Length / 5); // Allow 1 typo or 20% of the string length

			// This might be a stupid multiple artist or "featuring" case, try splitting and checking just the first part of the guess/answer
			if (distance > maxAllowedDistance && (answer.Contains(',') || answer.Contains(';') || answer.Contains('(')))
			{
				return AreCloseEnough(
					answer.Split(new char[] { ',', ';', '(' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim(),
					guess.Split(new char[] { ',', ';', '(' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim());
			}

			return distance <= maxAllowedDistance;
		}

		/// <summary>
		/// Typo-tolerant comparison using Levenshtein distance.
		/// </summary>
		/// <param name="answer">The correct answer</param>
		/// <param name="guess">The sloppy guess</param>
		/// <returns>Roughly the count of differences</returns>
		private static int LevenshteinDistance(string answer, string guess)
		{
			ArgumentNullException.ThrowIfNull(answer);
			ArgumentNullException.ThrowIfNull(guess);

			int n = answer.Length;
			int m = guess.Length;

			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			var dp = new int[n + 1, m + 1];

			for (int i = 0; i <= n; i++)
			{
				dp[i, 0] = i;
			}

			for (int j = 0; j <= m; j++)
			{
				dp[0, j] = j;
			}

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = answer[i - 1] == guess[j - 1] ? 0 : 1;

					dp[i, j] = Math.Min(
						Math.Min(dp[i - 1, j] + 1,  // Deletion
						dp[i, j - 1] + 1),          // Insertion
						dp[i - 1, j - 1] + cost     // Substitution
					);
				}
			}

			return dp[n, m];
		}

		/// <summary>
		/// Trim and lowercase the answer, replace diacritics, remove punctuation, and don't count "The " at the start
		/// </summary>
		/// <param name="input">Input string to normalize</param>
		/// <returns>A standardized string</returns>
		private static string NormalizeString(string input)
		{
			input = input.Trim().ToLowerInvariant();
			if (input.StartsWith("the "))
			{
				input = input.Substring(4);
			}

			// Consider ampersand as "and"
			input = input.Replace("&", " and ");

			// Replace diacritics with basic ASCII equivalents (give the user a break)
			input = input.Normalize(System.Text.NormalizationForm.FormD);

			//Remove punctuation and extra spaces
			input = new string([.. input.Where(c => !char.IsPunctuation(c))]);
			input = MyRegex().Replace(input, " ");
			return input;
		}

		/// <summary>
		/// Returns a compiled regular expression that matches one or more whitespace characters.
		/// </summary>
		[System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
		private static partial System.Text.RegularExpressions.Regex MyRegex();
	}
}
