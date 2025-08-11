namespace MaimThatTune.Server.Controllers
{
    public partial class MusicController
    {
        /// <summary>
        /// Model to store track metadata.
        /// </summary>
        public class TrackMetadata
        {
            /// <summary>
            /// The artist name.
            /// </summary>
            public string Artist { get; set; } = string.Empty;

            /// <summary>
            /// The track title.
            /// </summary>
            public string Track { get; set; } = string.Empty;
        }
    }
}