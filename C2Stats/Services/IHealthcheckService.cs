using Microsoft.Extensions.Options;

namespace C2Stats.Services
{
	// todo: reuse from dddns-updater
	public interface IHealthcheckService
	{
		public Task<bool> Success(CancellationToken cancellationToken = default);
		
		public Task<bool> Start(CancellationToken cancellationToken = default);
		
		public Task<bool> Failure(string message, CancellationToken cancellationToken = default);
		
		public Task<bool> Log(string message, CancellationToken cancellationToken = default);
	}

	public class HealthcheckIoOptions : IConfigOptions
	{
		public static string SectionName  => "HealthcheckIo";

		public string? Url { get; set; }
	}
	
	public class HealthcheckIoService(ILogger<HealthcheckIoService> logger,
		IOptions<HealthcheckIoOptions> options, IHttpClientFactory httpClientFactory) : IHealthcheckService
	{
		public async Task<bool> Success(CancellationToken cancellationToken = default)
		{
			return await Ping(string.Empty, null, cancellationToken);
		}

		public async Task<bool> Start(CancellationToken cancellationToken = default)
		{
			return await Ping("/start", null, cancellationToken);
		}

		public async Task<bool> Failure(string message, CancellationToken cancellationToken = default)
		{
			return await Ping("/fail", message, cancellationToken);
		}

		public async Task<bool> Log(string message, CancellationToken cancellationToken = default)
		{
			return await Ping("/log", message, cancellationToken);
		}

		private async Task<bool> Ping(string method, string? message, CancellationToken cancellationToken)
		{
			var settings = options.Value;
			
			if (settings?.Url == null)
			{
				logger.LogDebug("healthcheck.io is not configured, ping not sent.");
					
				return false;
			}

			try
			{
				using (var client = httpClientFactory.CreateClient())
				{
					var uriBuilder = new UriBuilder($"{settings.Url}{method}");

					var content = new StringContent(message ?? string.Empty);
					
					var response = await client.PostAsync(uriBuilder.Uri, content, cancellationToken);

					response.EnsureSuccessStatusCode();
					
					return true;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to ping {Method}", method);
				
				return false;
			}
		}
	}
}