namespace MusicFinderTests
{
	[TestClass]
	public sealed class RandomTrackPickerTests
	{
		[TestMethod]
		public async Task GetRandomTrack_FindsANonNullPath()
		{
			var picker = new MusicFinder.RandomTrackPicker();
			string? track;

			// Get the config value using the helper
			string? configValue = MusicFinder.ConfigurationHelper.GetMusicFolderRoot();

			// If root file path isn't configured correctly, create a temporary folder/path so the test can run
			if (string.IsNullOrEmpty(configValue) || !Directory.Exists(configValue))
			{
				var tempMusicFolder = CreateTestMusicStructure();
				track = await picker.GetRandomTrackAsync(tempMusicFolder);
			}
			else
			{
				track = await picker.GetRandomTrackAsync();
			}

			Assert.IsNotNull(track);
			Assert.IsTrue(File.Exists(track), "The selected track file does not exist.");
			Console.WriteLine($"Selected track: {track}");
		}

		private string CreateTestMusicStructure()
		{
			// Create a temporary directory structure for testing
			var tempDir = Path.Combine(Path.GetTempPath(), "TestMusic", Guid.NewGuid().ToString());
			var artistDir = Path.Combine(tempDir, "TestArtist");
			var albumDir = Path.Combine(artistDir, "TestAlbum");

			// Create all directories first
			Directory.CreateDirectory(albumDir);

			// Create a dummy MP3 file for testing
			var mp3File = Path.Combine(albumDir, "TestSong.mp3");
			File.WriteAllText(mp3File, "dummy mp3 content for testing");

			Console.WriteLine($"Created test music structure at: {tempDir}");
			return tempDir;
		}

		[TestCleanup]
		public void Cleanup()
		{
			// Clean up any temporary test directories
			var tempMusicDir = Path.Combine(Path.GetTempPath(), "TestMusic");
			if (Directory.Exists(tempMusicDir))
			{
				try
				{
					Directory.Delete(tempMusicDir, true);
				}
				catch
				{
					// Ignore cleanup errors
				}
			}
		}
	}
}
