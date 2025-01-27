using C2Stats.Models;

namespace C2Stats.Services
{
	public interface IWodDownloader
	{
		Task<WodResult> Download(DateOnly date, string wodType, CancellationToken cancellationToken);
	}
	
	public class WodDownloader(IHttpClientFactory httpClientFactory, IWodParser parser) : IWodDownloader
	{
		public async Task<WodResult> Download(DateOnly date, string wodType, CancellationToken cancellationToken)
		{
			var httpClient = httpClientFactory.CreateClient();

			var result = new WodResult();
			
			var pageNo = 1;
			
			while (pageNo <= 200)
			{
				var url = $"https://log.concept2.com/wod/{date:yyyy-MM-dd}/{wodType}?page={pageNo}";
				
				var response = await httpClient.GetAsync(url, cancellationToken);
				
				response.EnsureSuccessStatusCode();
				
				var html = await response.Content.ReadAsStringAsync(cancellationToken);
				
				var page = parser.Parse(html);

				if (page.Success == true)
				{
					if (pageNo == 1)
					{
						result.Date = date;
						result.Type = wodType;
						result.Name = page.Name;
						result.Description = page.Description;
					}

					foreach (var item in page.Items)
					{
						if (result.Items.FirstOrDefault(x => x.Id == item.Id) == null)
						{
							result.Items.Add(item);
						}
					}
					
					pageNo++;
				}
				else
				{
					break;
				}
			}
			
			return result;
		}
	}
}