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
		private static readonly string[] InvalidArtistValues = { "Unknown", "Various", "Various Artists", string.Empty };
		private static readonly string[] InvalidTitleValues = { "Unknown", string.Empty };

		/// <summary>
		/// Streams a 5-second segment of a random MP3 file and returns a track ID.
		/// </summary>
		/// <returns>MP3 audio segment and track ID</returns>
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		[HttpGet("random-track")]
		public async Task<IActionResult> GetRandomSegmentAsync()
		{
			const int maxRetries = 10; // Prevent infinite loops

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				var trackPath = await _picker.GetRandomTrackAsync().ConfigureAwait(false);
				if (trackPath == null)
				{
					continue;
				}

				var mp3Info = new Mp3Info(trackPath);

				// Check if artist and title are valid
				if (InvalidArtistValues.Contains(mp3Info.Artist, StringComparer.OrdinalIgnoreCase) ||
					InvalidTitleValues.Contains(mp3Info.Title, StringComparer.OrdinalIgnoreCase))
				{
					continue; // Try another track
				}

				var fileServer = new FileServer();
				var stream = await fileServer.GetFile(trackPath).ConfigureAwait(false);

				if (stream == null)
				{
					continue; // Try another track
				}

				var trackId = Guid.NewGuid().ToString();

				_trackMetadataMap[trackId] = new TrackMetadata
				{
					Artist = mp3Info.Artist,
					Track = mp3Info.Title
				};

				Response.Headers.Append("X-Track-Id", trackId);
				return File(stream, "audio/mpeg", enableRangeProcessing: true);
			}

			// If we've tried maxRetries times and still no valid track, return NotFound
			return NotFound("No valid tracks found after multiple attempts");
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

			var isArtistCorrect = SloppyAnswerComparer.AreCloseEnough(request.Guess, metadata.Artist);
			var isTrackCorrect = SloppyAnswerComparer.AreCloseEnough(request.Guess, metadata.Track);
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
