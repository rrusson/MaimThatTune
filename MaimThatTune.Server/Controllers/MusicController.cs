using System.Collections.Concurrent;

using MaimThatTune.Server.Code;

using Microsoft.AspNetCore.Mvc;

using MusicFinder;

namespace MaimThatTune.Server.Controllers
{
	/// <summary>
	/// Provides endpoints for streaming random MP3 segments and artist/track guessing.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public partial class MusicController : ControllerBase
	{
		private static readonly ConcurrentDictionary<string, TrackMetadata> _trackMetadataMap = new();
		private static readonly RandomTrackPicker _picker = new();

		/// <summary>
		/// Streams a 5-second segment of a random MP3 file and returns a track ID.
		/// </summary>
		/// <returns>MP3 audio segment and track ID</returns>
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		[HttpGet("random-segment")]
		public async Task<IActionResult> GetRandomSegmentAsync()
		{
			var trackPath = await _picker.GetRandomTrackAsync().ConfigureAwait(false);

			var fileServer = new FileServer();
			var stream = await fileServer.GetFile(trackPath).ConfigureAwait(false);

			if (stream == null)
			{
				return NotFound();
			}

			var mp3Info = new Mp3Info(trackPath!);
			var trackInfo = mp3Info.GetAllProperties();
			var artist = trackInfo.FirstPerformer ?? "Unknown";
			var track = trackInfo.Title ?? "Unknown";
			var trackId = Guid.NewGuid().ToString();
			
			_trackMetadataMap[trackId] = new TrackMetadata
			{
				Artist = artist,
				Track = track
			};

			Response.Headers.Append("X-Track-Id", trackId);
			return File(stream, "audio/mpeg", enableRangeProcessing: true);
		}

		/// <summary>
		/// Checks the user's guess for the artist or track of the current track.
		/// </summary>
		/// <param name="request">Guess request containing track ID and guess</param>
		/// <returns>Result with correctness, artist name, and track title</returns>
		[HttpPost("guess")]
		public IActionResult GuessArtist([FromBody] GuessRequest request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.TrackId) || string.IsNullOrWhiteSpace(request.Guess))
			{
				return BadRequest();
			}

			if (!_trackMetadataMap.TryGetValue(request.TrackId, out var metadata))
			{
				return NotFound();
			}

			var guess = request.Guess.Trim();
			var isArtistCorrect = string.Equals(guess, metadata.Artist, StringComparison.OrdinalIgnoreCase);
			var isTrackCorrect = string.Equals(guess, metadata.Track, StringComparison.OrdinalIgnoreCase);
			var isCorrect = isArtistCorrect || isTrackCorrect;

			// Remove the track from the map after guessing
			_trackMetadataMap.TryRemove(request.TrackId, out _);

			return Ok(new GuessResult
			{
				IsCorrect = isCorrect,
				Artist = metadata.Artist,
				Track = metadata.Track
			});
		}
	}
}
