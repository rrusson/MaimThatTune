using MusicFinder;

using NAudio.Wave;

namespace MaimThatTune.Server.Code
{
	public class FileServer
	{
		/// <summary>
		/// Retrieves a stream of audio data from the specified track path
		/// </summary>
		/// <param name="trackPath">Full path to the audio file</param>
		/// <returns>The complete audio stream</returns>
		public async Task<MemoryStream?> GetAudioAsync(string? trackPath)
		{
			if (trackPath == null || !File.Exists(trackPath))
			{
				return null;
			}

			var secondsToStreamStr = ConfigurationHelper.GetSecondsAudioToStream();

			// If 0 or not set, stream the entire file
			if (!int.TryParse(secondsToStreamStr, out int secondsToStream) || secondsToStream <= 0)
			{
				var stream = new MemoryStream();
				await using (var fs = File.OpenRead(trackPath))
				{
					await fs.CopyToAsync(stream).ConfigureAwait(false);
				}
				stream.Position = 0;
				return stream;
			}

			// Use NAudio to stream only the first N seconds (supports MP3/WAV)
			return GetTruncatedAudio(trackPath, secondsToStream);
		}

		/// <summary>
		/// Retrieves a truncated stream of audio data from the specified track path
		/// </summary>
		/// <param name="trackPath">Full path to an MP3 audio file</param>
		/// <param name="secondsToStream">Number of seconds to stream from the start of the audio file</param>
		/// <returns>A truncated audio stream</returns>
		public static MemoryStream? GetTruncatedAudio(string trackPath, int secondsToStream)
		{
			if (string.IsNullOrEmpty(trackPath) || !File.Exists(trackPath) || !trackPath.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}

			using var reader = new Mp3FileReader(trackPath);
			var output = new MemoryStream();
			double totalSeconds = 0;
			Mp3Frame? frame;

			while ((frame = reader.ReadNextFrame()) != null)
			{
				output.Write(frame.RawData, 0, frame.RawData.Length);
				totalSeconds += (double)frame.SampleCount / frame.SampleRate;
				if (totalSeconds >= secondsToStream)
				{
					break;
				}
			}

			output.Position = 0;
			return output;
		}
	}
}
