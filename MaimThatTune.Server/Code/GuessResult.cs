namespace MaimThatTune.Server.Controllers
{
	public partial class MusicController
	{
		/// <summary>
		/// Result model for artist and track guesses
		/// </summary>
		public class GuessResult
		{
			/// <summary>
			/// True if the guess was correct
			/// </summary>
			public bool IsCorrect { get; set; }

			/// <summary>
			/// The actual artist name
			/// </summary>
			public string Artist { get; set; } = string.Empty;

			/// <summary>
			/// The actual track title
			/// </summary>
			public string Track { get; set; } = string.Empty;
		}
	}
}
