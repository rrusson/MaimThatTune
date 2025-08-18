using Microsoft.Extensions.Configuration;

namespace MusicFinder
{
	public static class ConfigurationHelper
	{
		private static IConfiguration? _configuration;
		private static readonly Lock _lock = new();

		public static string? GetMusicFolderRoot()
		{
			return GetConfiguration()["MusicFolderRoot"];
		}

		public static string? GetSecondsAudioToStream()
		{
			return GetConfiguration()["SecondsAudioToStream"];
		}

		private static IConfiguration GetConfiguration()
		{
			if (_configuration != null)
			{
				return _configuration;
			}

			using (_lock.EnterScope())
			{
				if (_configuration == null)
				{
					var builder = new ConfigurationBuilder()
						.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
						.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

					_configuration = builder.Build();
				}
			}

			return _configuration;
		}
	}
}