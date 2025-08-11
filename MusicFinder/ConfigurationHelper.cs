using Microsoft.Extensions.Configuration;

namespace MusicFinder
{
	public static class ConfigurationHelper
	{
		private static IConfiguration? _configuration;
		private static readonly object _lock = new object();

		public static IConfiguration GetConfiguration()
		{
			if (_configuration != null)
			{
				return _configuration;
			}

			var myLock = new Lock();

			using (myLock.EnterScope())
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

		public static string? GetMusicFolderRoot()
		{
			return GetConfiguration()["MusicFolderRoot"];
		}
	}
}