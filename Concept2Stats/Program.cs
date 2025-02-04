using Concept2Stats.Models;
using Concept2Stats.Services;
using Concept2Stats.Services.Jobs;
using Quartz;
using Serilog;
using Serilog.Extensions.Logging;

namespace Concept2Stats
{
	public abstract class Program
	{
		public const string ConfigFilePath = "./data/settings.json";
		
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateBootstrapLogger();
			
			var logger = new SerilogLoggerFactory(Log.Logger).CreateLogger<Program>();

			try
			{
				var builder = WebApplication.CreateBuilder(args);

				builder.Configuration.AddJsonFile(ConfigFilePath, true, true);
				
				builder
					.Configure<AppOptions>()
					.Configure<AppriseOptions>()
					.Configure<HealthcheckIoOptions>();
				
				builder.Services
					.AddSerilog((services, lc) => lc
						.ReadFrom.Configuration(builder.Configuration)
						.ReadFrom.Services(services));

				builder.Services
					.AddHttpClient()
					.AddServiceKeyProvider()
					.AddHostedService<BackgroundMessenger>()
					.AddSingleton<IMessageSender, AppriseMessageSender>()
					.AddSingleton<IHealthcheckService, HealthcheckIoService>()
					.AddTransient<IWodFileStorage, WodFileStorage>()
					.AddTransient<IWodDownloader, WodDownloader>()
					.AddTransient<IWodParser, WodParser>()
					
					.AddQuartz(quartz =>
					{
						quartz.SetProperty("quartz.scheduler.interruptJobsOnShutdownWithWait", "true");
						
						quartz
							.AddJob<DownloadCurrentWodJob>(logger, builder.Configuration)
							.AddJob<DownloadYesterdayWodJob>(logger, builder.Configuration)
							.AddJob<DownloadArchiveWodJob>(logger, builder.Configuration);
					})
					.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

				builder.Services.AddControllersWithViews();

				var app = builder.Build();

				app.UseSerilogRequestLogging();
				
				if (app.Environment.IsDevelopment() == false)
				{
					app.UseExceptionHandler("/Home/Error");
					// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
					app.UseHsts();
				}

				app.UseHttpsRedirection();
				app.UseStaticFiles();

				app.UseRouting();

				app.UseAuthorization();

				app.MapControllerRoute(
					"default",
					"{controller=Home}/{action=Index}/{id?}");

				app.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}