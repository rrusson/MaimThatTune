namespace MaimThatTune.Server.Controllers
{
public partial class MusicController
	{
		/// <summary>
		/// Request model for artist guess.
		/// </summary>
		public class GuessRequest
        {
            /// <summary>
            /// The track ID provided by the random-segment endpoint.
            /// </summary>
            public string TrackId { get; set; } = string.Empty;
            /// <summary>
            /// The user's guess for the artist.
            /// </summary>
            public string Guess { get; set; } = string.Empty;
        }
    }
}
