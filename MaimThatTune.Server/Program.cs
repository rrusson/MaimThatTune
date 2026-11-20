using Serilog;
using Serilog.Debugging;
using Serilog.Events;

namespace MaimThatTune.Server
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			SelfLog.Enable(TextWriter.Synchronized(Console.Error));
			var errorLogPath = builder.Configuration["ErrorLogPath"]
				?? @"C:\inetpub\logs\AppLogs\MaimThatTuneErrors.log";
			var errorLogger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.WriteTo.File(
					errorLogPath,
					restrictedToMinimumLevel: LogEventLevel.Error,
					outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
					fileSizeLimitBytes: 10 * 1024 * 1024,
					rollOnFileSizeLimit: true,
					retainedFileCountLimit: 10,
					shared: true)
				.CreateLogger();
			builder.Logging.AddSerilog(errorLogger, dispose: true);

			builder.Services.AddControllers();
			builder.Services.AddOpenApi();

			var app = builder.Build();
			app.UseDefaultFiles();
			app.MapStaticAssets();

			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			app.UseHttpsRedirection();
			app.UseAuthorization();
			app.MapControllers();
			app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}
