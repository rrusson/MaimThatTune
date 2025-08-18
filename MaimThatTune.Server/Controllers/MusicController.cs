using System.Collections.Concurrent;

using MaimThatTune.Server.Code;

using Microsoft.AspNetCore.Mvc;

using MusicFinder;

namespace MaimThatTune.Server.Controllers
{
	/// <summary>
	/// Provides endpoints for streaming random MP3 segments, getting available music genres, and artist/track guessing
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public partial class MusicController : ControllerBase
	{
		private static readonly ConcurrentDictionary<string, Mp3Info> _trackMetadataMap = new();

		/// <summary>
		/// Gets available genres from the music directory
		/// </summary>
		/// <returns>List of available genres</returns>
		[HttpGet("genres")]
		public async Task<IActionResult> GetGenresAsync()
		{
			if (!ConfigurationHelper.GetUseCategories())
			{
				return Ok(new List<string>());
			}

			var genres = await RandomTrackPicker.GetAvailableGenresAsync().ConfigureAwait(false);
			return Ok(genres);
		}

		/// <summary>
		/// Streams a random MP3 file and returns a track ID
		/// </summary>
		/// <param name="genre">Optional genre filter</param>
		/// <returns>MP3 audio with a track ID header</returns>
		[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
		[HttpGet("random-track")]
		public async Task<IActionResult> GetRandomSegmentAsync([FromQuery] string? genre = null)
		{
			const int maxRetries = 10;

			for (int attempt = 0; attempt < maxRetries; attempt++)
			{
				var trackPath = await RandomTrackPicker.GetRandomTrackAsync(null, genre).ConfigureAwait(false);
				if (trackPath == null)
				{
					continue;
				}

				var mp3Info = new Mp3Info(trackPath);
				if (mp3Info.IsInvalidArtist() || mp3Info.IsInvalidTitle())
				{
					continue; // Try another track
				}

				var fileServer = new FileServer();
				var stream = await fileServer.GetAudioAsync(trackPath).ConfigureAwait(false);

				if (stream == null)
				{
					continue; // Try another track
				}
#if DEBUG
				Task.Delay(5000).Wait(); // Simulate some delay to view wait behavior when running locally
#endif
				var trackId = Guid.NewGuid().ToString();
				_trackMetadataMap[trackId] = mp3Info;

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

			var isCorrect = SloppyAnswerComparer.AreCloseEnough(request.Guess, metadata.Artist)
				|| SloppyAnswerComparer.AreCloseEnough(request.Guess, metadata.Title);

			// Remove the track from the map after guessing
			_trackMetadataMap.TryRemove(request.TrackId, out _);

			return Ok(new GuessResult
			{
				IsCorrect = isCorrect,
				Artist = metadata.Artist,
				Track = metadata.Title
			});
		}
	}
}
