namespace MaimThatTune.Server.Code
{
	public class FileServer
	{
		public async Task<MemoryStream?> GetFile(string? trackPath)
		{
			if (trackPath == null || !System.IO.File.Exists(trackPath))
			{
				return null;
			}

			var segmentLength = TimeSpan.FromSeconds(5);
			var stream = new MemoryStream();

			// For simplicity, stream the first 5 seconds (may not be frame-accurate)
			// TagLib# does not support segment extraction, so we send the full file for now
			// In production, use a library like NAudio or ffmpeg for accurate segmenting
			await using (var fs = System.IO.File.OpenRead(trackPath))
			{
				await fs.CopyToAsync(stream).ConfigureAwait(false);
			}

			stream.Position = 0;
			return stream;
		}
	}
}
