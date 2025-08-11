using Microsoft.Extensions.Configuration;

namespace MusicFinderTests
{
	[TestClass]
	public sealed class MusicFinderTests
	{
		[TestMethod]
		public void ModernConfiguration_ReadsAppsettingsJson()
		{
			// Test the modern .NET configuration system
			var builder = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			var config = builder.Build();
			string? modernConfigValue = config["MusicFolderRoot"];

			Console.WriteLine($"Config value read: '{modernConfigValue}'");

			Assert.IsNotNull(modernConfigValue);
			Assert.AreEqual(@"C:\Music\", modernConfigValue);
		}

		[TestMethod]
		public void ConfigurationHelper_ReadsConfiguration()
		{
			// Test our configuration helper
			string? helperConfigValue = MusicFinder.ConfigurationHelper.GetMusicFolderRoot();
			Console.WriteLine($"ConfigurationHelper value read: '{helperConfigValue}'");

			Assert.IsNotNull(helperConfigValue);
			Assert.AreEqual(@"C:\Music\", helperConfigValue);
		}

		[TestMethod]
		public async Task GetRandomTrack_FindsANonNullPath()
		{
			var picker = new MusicFinder.RandomTrackPicker();
			string? track;

			// Get the config value using the helper
			string? configValue = MusicFinder.ConfigurationHelper.GetMusicFolderRoot();
			Console.WriteLine($"Config value: '{configValue}'");

			// If the config path doesn't exist or isn't accessible, create a test path
			if (string.IsNullOrEmpty(configValue) || !Directory.Exists(configValue))
			{
				// Create a temporary test music folder structure for testing
				var tempMusicFolder = CreateTestMusicStructure();
				track = await picker.GetRandomTrackAsync(tempMusicFolder);
			}
			else
			{
				// Use the configured path
				track = await picker.GetRandomTrackAsync();
			}

			Assert.IsNotNull(track);
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
