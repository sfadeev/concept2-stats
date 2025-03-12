using System.Text.Json.Serialization;
using C2Stats.Models;
using C2Stats.Services;
using C2Stats.Services.Jobs;
using LinqToDB.Data;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Serilog;
using Serilog.Extensions.Logging;

namespace C2Stats
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

				builder.Services.Configure<JsonOptions>(options =>
				{
					options.JsonSerializerOptions.WriteIndented = false;
					options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
				});
				
				builder.Services
					.AddSerilog((services, lc) => lc
						.ReadFrom.Configuration(builder.Configuration)
						.ReadFrom.Services(services))
					.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
				
				DataConnection.DefaultSettings = new DbSettingsProvider(builder.Configuration);

				builder.Services
					.AddHttpClient()
					.AddServiceKeyProvider()
					.AddHostedService<BackgroundMessenger>()
					
					.AddSingleton<IMessageSender, AppriseMessageSender>()
					.AddSingleton<IHealthcheckService, HealthcheckIoService>()
					.AddSingleton<IProfileFileStorage, ProfileFileStorage>()
					.AddSingleton<ITimeZoneDateProvider, SimpleTimeZoneDateProvider>()
					
					.AddScoped<ICountryProvider, DbCountryProvider>()
					// .AddTransient<ICountryProvider, DefaultCountryProvider>()
					
					.AddTransient<ICountryDownloader, CountryDownloader>()
					.AddTransient<IWodFileStorage, WodFileStorage>()
					.AddTransient<IWodDownloader, WodDownloader>()
					.AddTransient<IWodParser, WodParser>()
					.AddTransient<ICountryDbStorage, CountryDbStorage>()
					.AddTransient<IProfileDbStorage, ProfileDbStorage>()
					.AddTransient<IWodDbStorage, WodDbStorage>()
					.AddTransient<IWodStatsService, WodStatsService>()
					
					.AddQuartz(quartz =>
					{
						quartz.SetProperty("quartz.scheduler.interruptJobsOnShutdownWithWait", "true");
						
						quartz
							.AddJob<DownloadCurrentWodJob>(logger, builder.Configuration)
							.AddJob<DownloadYesterdayWodJob>(logger, builder.Configuration)
							.AddJob<DownloadInconsistentWodJob>(logger, builder.Configuration)
							.AddJob<DownloadArchiveWodJob>(logger, builder.Configuration);
					})
					.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

				builder.Services
					.AddControllers()
					.AddJsonOptions(options =>
					{
						options.JsonSerializerOptions.WriteIndented = false;
						options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
					});
				
				/*builder.Services.AddSpaStaticFiles(config => // ???
				{
					config.RootPath = "wwwroot";
				});*/
				
				var app = builder.Build();

				app.UseSerilogRequestLogging();
				
				// if (app.Environment.IsDevelopment() == false)
				{
					// app.UseSpaStaticFiles(); // ???
					
					app.UseExceptionHandler("/Home/Error");
					// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
					app.UseHsts();
				}

				// app.UseHttpsRedirection();
				app.UseStaticFiles();

				app.UseRouting();
				// app.UseAuthorization();
				
				app.MapControllerRoute(
					"default",
					"{controller=Home}/{action=Index}/{id?}");
				
				app.UseSpa(spa =>
				{
					/*if (app.Environment.IsDevelopment())
					{
						spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
					}*/
				});
				
				app.MapGet("/api/date", () =>
				{
					return Results.Json(new { date = DateTime.UtcNow.ToString("O") });
				});
				
				// app.MapFallbackToFile("index.html"); // serves react app for all routes
				
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