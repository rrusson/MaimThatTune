namespace MusicFinder
{
	public class Mp3Info
	{
		private const string Unknown = "Unknown";
		private static readonly string[] _invalidArtistValues = { "Unknown", "Various", "Various Artists", "-" };
		private static readonly string[] _invalidTitleValues = { "Unknown", "-" };

		/// <summary>
		/// The file path of the MP3 file.
		/// </summary>
		public string FilePath { get; }

		/// <summary>
		/// The title of the track.
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// The artist of the track.
		/// </summary>
		public string Artist { get; }

		/// <summary>
		/// The album of the track.
		/// </summary>
		public string Album { get; }

		/// <summary>
		/// The genre of the track.
		/// </summary>
		public string Genre { get; }

		/// <summary>
		/// The duration of the track.
		/// </summary>
		public TimeSpan Duration { get; }

		public Mp3Info(string filePath)
		{
			FilePath = filePath;

			using var file = TagLib.File.Create(filePath);

			Title = file.Tag.Title ?? Unknown;
			Artist = file.Tag.FirstPerformer ?? Unknown;
			Album = file.Tag.Album ?? Unknown;
			Genre = file.Tag.FirstGenre ?? Unknown;
			Duration = file.Properties.Duration;
		}

		public bool IsInvalidArtist()
		{
			return string.IsNullOrWhiteSpace(Artist) || _invalidArtistValues.Contains(Artist, StringComparer.OrdinalIgnoreCase);
		}

		public bool IsInvalidTitle()
		{
			return string.IsNullOrWhiteSpace(Title) || _invalidTitleValues.Contains(Title, StringComparer.OrdinalIgnoreCase);
		}
	}
}
