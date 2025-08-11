namespace MaimThatTune.Server.Controllers
{
public partial class MusicController
	{
		/// <summary>
		/// Result model for artist guess.
		/// </summary>
		public class GuessResult
        {
            /// <summary>
            /// Whether the guess was correct.
            /// </summary>
            public bool IsCorrect { get; set; }
            /// <summary>
            /// The actual artist name.
            /// </summary>
            public string Artist { get; set; } = string.Empty;
        }
    }
}
