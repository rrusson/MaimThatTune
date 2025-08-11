using TagLib;

namespace MusicFinder
{
	public class Mp3Info
	{
		private const string? Unknown = null;
		private readonly string _filePath;

		public Mp3Info(string filePath)
		{
			_filePath = filePath;
		}

		public Tag GetAllProperties()
		{
			using var file = TagLib.File.Create(_filePath);
			return file.Tag;
		}

		public string GetTitle() => GetAllProperties().Title ?? nameof(Unknown);

		public string GetArtist() => GetAllProperties().FirstPerformer ?? nameof(Unknown);

		public string GetAlbum() => GetAllProperties().Album ?? nameof(Unknown);

		public string GetGenre() => GetAllProperties().FirstGenre ?? nameof(Unknown);

		public TimeSpan GetDuration()
		{
			using var file = TagLib.File.Create(_filePath);
			return file.Properties.Duration;
		}
	}
}
