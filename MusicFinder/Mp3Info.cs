namespace MusicFinder
{
	public class Mp3Info
	{
		private const string Unknown = "Unknown";

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
	}
}
