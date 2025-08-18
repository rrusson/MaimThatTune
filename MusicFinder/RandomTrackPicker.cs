namespace MusicFinder
{
	public static class RandomTrackPicker
	{
		private static readonly Random _random = new();
		private static readonly string[] _invalidGenres = { "$RECYCLE.BIN", "_Incoming", "_Playlists", "Music Cache", "Uncategorized", "Utilities", "Various Artists", };


		/// <summary>
		/// Gets available genres (first-level directories) from the root music folder
		/// </summary>
		/// <param name="musicFolderRoot">Optional override for the music folder root. If null, reads from configuration</param>
		/// <returns>Array of genre names</returns>
		public static async Task<string?[]> GetAvailableGenresAsync(string? musicFolderRoot = null)
		{
			musicFolderRoot ??= ConfigurationHelper.GetMusicFolderRoot();

			if (string.IsNullOrEmpty(musicFolderRoot) || !Directory.Exists(musicFolderRoot))
			{
				return [];
			}

			return await Task.Run(() =>
			{
				try
				{
					var directories = Directory.GetDirectories(musicFolderRoot)
						.Select(Path.GetFileName)
						.Where(name => !_invalidGenres.Contains(name, StringComparer.OrdinalIgnoreCase))
						.OrderBy(name => name)
						.ToArray();

					return directories!;
				}
				catch (UnauthorizedAccessException)
				{
					return [];
				}
				catch (DirectoryNotFoundException)
				{
					return [];
				}
			});
		}

		/// <summary>
		/// Returns a random MP3 track from a randomly selected folder within the music directory structure.
		/// </summary>
		/// <param name="musicFolderRoot">Optional override for the music folder root. If null, reads from configuration.</param>
		/// <param name="genre">Optional genre filter. If provided, only searches within that genre subfolder.</param>
		/// <returns>The full path to an MP3 file</returns>
		/// <exception cref="DirectoryNotFoundException">Thrown if the root directory (from config) isn't found</exception>
		public static async Task<string?> GetRandomTrackAsync(string? musicFolderRoot = null, string? genre = null)
		{
			musicFolderRoot ??= ConfigurationHelper.GetMusicFolderRoot();

			if (string.IsNullOrEmpty(musicFolderRoot) || !Directory.Exists(musicFolderRoot))
			{
				throw new DirectoryNotFoundException($"Music folder root not found or doesn't exist: {musicFolderRoot}");
			}

			// If genre is specified and not "ALL", look only in that genre subfolder
			if (!string.IsNullOrEmpty(genre) && genre != "ALL")
			{
				var genrePath = Path.Combine(musicFolderRoot, genre);
				if (Directory.Exists(genrePath))
				{
					musicFolderRoot = genrePath;
				}
			}

			var leafFolder = await FindRandomMp3FolderAsync(musicFolderRoot);

			if (leafFolder == null)
			{
				return null;
			}

			var mp3Files = await GetMp3FilesFromFolderAsync(leafFolder);

			if (mp3Files.Count() == 0)
			{
				return null;
			}

			int randomIndex = _random.Next(mp3Files.Count());
			return mp3Files[randomIndex];
		}

		private static async Task<string?> FindRandomMp3FolderAsync(string currentPath)
		{
			return await Task.Run(() => FindRandomLeafFolder(currentPath));
		}

		private static string? FindRandomLeafFolder(string currentPath)
		{
			try
			{
				var subdirectories = Directory.GetDirectories(currentPath);

				// If no subdirectories, this is a potential leaf folder
				if (subdirectories.Length == 0)
				{
					string[] mp3Files = Directory.GetFiles(currentPath, "*.mp3", SearchOption.TopDirectoryOnly);
					return mp3Files.Length > 0 ? currentPath : null;
				}

				// Randomly select a subdirectory and recurse
				var attempts = 0;
				var maxAttempts = subdirectories.Length * 3;

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
					// Just skip this folder for these exceptions
					catch (UnauthorizedAccessException) { }
					catch (DirectoryNotFoundException) { }
					catch (PathTooLongException) { }

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

		private static async Task<string[]> GetMp3FilesFromFolderAsync(string folderPath)
		{
			string[] mp3Files = [];

			await Task.Run(() =>
			{
				try
				{
					// Get MP3 files only from the specified folder (not recursive)
					mp3Files = Directory.GetFiles(folderPath, "*.mp3", SearchOption.TopDirectoryOnly);
				}
				// Ignore these exceptions
				catch (UnauthorizedAccessException) { }
				catch (DirectoryNotFoundException) { }
				catch (PathTooLongException) { }
			});

			return mp3Files;
		}
	}
}
