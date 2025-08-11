namespace MusicFinder
{
	public class RandomTrackPicker
	{
		private static readonly Random _random = new();

		/// <summary>
		/// Returns a random MP3 track from a randomly selected folder within the music directory structure.
		/// </summary>
		/// <param name="musicFolderRoot">Optional override for the music folder root. If null, reads from configuration.</param>
		/// <returns>The full path to an MP3 file</returns>
		/// <exception cref="DirectoryNotFoundException">Thrown if the root directory (from config) isn't found</exception>
		public async Task<string?> GetRandomTrackAsync(string? musicFolderRoot = null)
		{
			// Use provided path or read from modern configuration
			musicFolderRoot ??= ConfigurationHelper.GetMusicFolderRoot();

			if (string.IsNullOrEmpty(musicFolderRoot) || !Directory.Exists(musicFolderRoot))
			{
				throw new DirectoryNotFoundException($"Music folder root not found or doesn't exist: {musicFolderRoot}");
			}

			// Find a random leaf folder that contains MP3 files
			var leafFolder = await FindRandomMp3FolderAsync(musicFolderRoot);

			if (leafFolder == null)
			{
				return null; // No folders with MP3 files found
			}

			// Get all MP3 files from the leaf folder and pick one randomly
			var mp3Files = await GetMp3FilesFromFolderAsync(leafFolder);

			if (mp3Files.Count == 0)
			{
				return null; // No MP3 files found in the leaf folder
			}

			// Return a random MP3 file
			int randomIndex = _random.Next(mp3Files.Count);
			return mp3Files[randomIndex];
		}

		private async Task<string?> FindRandomMp3FolderAsync(string currentPath)
		{
			return await Task.Run(() => FindRandomLeafFolder(currentPath));
		}

		private string? FindRandomLeafFolder(string currentPath)
		{
			try
			{
				// Get all subdirectories in the current path
				var subdirectories = Directory.GetDirectories(currentPath);

				// If no subdirectories, this is a potential leaf folder
				if (subdirectories.Length == 0)
				{
					// Check if this folder contains MP3 files
					string[] mp3Files = Directory.GetFiles(currentPath, "*.mp3", SearchOption.TopDirectoryOnly);
					return mp3Files.Length > 0 ? currentPath : null;
				}

				// Randomly select a subdirectory and recurse
				var attempts = 0;
				var maxAttempts = subdirectories.Length * 3; // Allow multiple attempts to find a valid path

				while (attempts < maxAttempts)
				{
					var randomIndex = _random.Next(subdirectories.Length);
					var selectedFolder = subdirectories[randomIndex];

					try
					{
						var result = FindRandomLeafFolder(selectedFolder);
						if (result != null)
						{
							return result;
						}
					}
					catch (UnauthorizedAccessException)
					{
						// Skip this folder and try another
					}
					catch (DirectoryNotFoundException)
					{
						// Skip this folder and try another
					}
					catch (PathTooLongException)
					{
						// Skip this folder and try another
					}

					attempts++;
				}

				// If we couldn't find a valid path through subdirectories, check if the current folder itself has MP3 files
				var currentFolderMp3s = Directory.GetFiles(currentPath, "*.mp3", SearchOption.TopDirectoryOnly);

				return currentFolderMp3s.Length > 0 ? currentPath : null;
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
			catch (PathTooLongException)
			{
				return null;
			}
		}

		private async Task<List<string>> GetMp3FilesFromFolderAsync(string folderPath)
		{
			var mp3Files = new List<string>();

			await Task.Run(() =>
			{
				try
				{
					// Get MP3 files only from the specified folder (not recursive)
					string[] files = Directory.GetFiles(folderPath, "*.mp3", SearchOption.TopDirectoryOnly);
					mp3Files.AddRange(files);
				}
				catch (UnauthorizedAccessException)
				{
					// Skip directories we don't have access to
				}
				catch (DirectoryNotFoundException)
				{
					// Skip directories that don't exist
				}
				catch (PathTooLongException)
				{
					// Skip paths that are too long
				}
			});

			return mp3Files;
		}
	}
}
