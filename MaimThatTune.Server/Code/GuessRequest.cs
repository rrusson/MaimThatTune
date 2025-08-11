namespace MaimThatTune.Server.Controllers
{
public partial class MusicController
	{
		/// <summary>
		/// Request model for artist or track guess.
		/// </summary>
		public class GuessRequest
        {
            /// <summary>
            /// The track ID provided by the random-segment endpoint.
            /// </summary>
            public string TrackId { get; set; } = string.Empty;
            /// <summary>
            /// The user's guess for the artist or track name.
            /// </summary>
            public string Guess { get; set; } = string.Empty;
        }
    }
}
