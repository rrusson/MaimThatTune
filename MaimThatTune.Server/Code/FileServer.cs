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

			// TODO: Let's use a library like NAudio or ffmpeg to only stream the first 10 seconds, to conserve bandwidth
			await using (var fs = System.IO.File.OpenRead(trackPath))
			{
				await fs.CopyToAsync(stream).ConfigureAwait(false);
			}

			stream.Position = 0;
			return stream;
		}
	}
}
