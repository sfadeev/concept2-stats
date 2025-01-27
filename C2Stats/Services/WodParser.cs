using System.Globalization;
using System.Web;
using C2Stats.Models;
using HtmlAgilityPack;

namespace C2Stats.Services
{
	public interface IWodParser
	{
		WodResult Parse(string html);
	}
	
	public class WodParser : IWodParser
	{
		private static readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("us");
		private static readonly string[] TimeSpanFormats = [ @"m\:ss\.f" ];

		public WodResult Parse(string html)
		{
			var result = new WodResult();

			var doc = new HtmlDocument();
			
			doc.LoadHtml(html);
			
			var content = doc.DocumentNode.SelectSingleNode("//section[@class='content']");
			
			if (content != null)
			{
				result.Name = content.SelectSingleNode("h3")?.InnerText;
				result.Description = content.SelectSingleNode("p/strong").InnerText;

				if (content.InnerText.Contains("Sorry, no results were found."))
				{
					result.Success = false;
				
					return result;
				}
				
				var rows = content.SelectNodes("table/tbody/tr");

				const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

				if (rows != null)
				{
					foreach (var row in rows)
					{
						var item = new WodResultItem();

						var cells = row.SelectNodes("td");

						for (var i = 0; i < cells.Count; i++)
						{
							var cell = cells[i];

							switch (i)
							{
								case 0:
									item.Position = ParseNumber(cell.InnerText);
									break;

								case 1:
									var anchor = cell.SelectSingleNode("a");

									if (anchor != null)
									{
										item.Name = ParseString(anchor.InnerText);

										var href = anchor.Attributes["href"]?.Value;

										if (href != null)
										{
											var idIndex = href.LastIndexOf("/", stringComparison);

											if (idIndex > 0) item.Id = ParseNumber(href[(idIndex + 1)..]);
										}
									}
									
									break;

								case 2:
									item.Age = ParseNumber(cell.InnerText);
									break;

								case 3:
									item.Location = ParseString(cell.InnerText);
									break;

								case 4:
									item.Country = ParseString(cell.InnerText);
									break;

								case 5:
									item.Affiliation = ParseString(cell.InnerText);
									break;

								case 6:
									if (cell.InnerText.EndsWith("m", stringComparison))
									{
										item.ResultMeters = ParseNumber(cell.InnerText[..^1]);
									}
									else
									{
										item.ResultTime = ParseTimeSpan(cell.InnerText);
									}
									
									break;

								case 7:
									item.Pace = ParseTimeSpan(cell.InnerText);
									break;
							}
						}

						result.Items.Add(item);
					}

					result.Success = true;
				}
			}

			return result;
		}

		private static string? ParseString(string value)
		{
			return string.IsNullOrEmpty(value) ? null : HttpUtility.HtmlDecode(value);
		}
		
		private static int? ParseNumber(string value)
		{
			return int.TryParse(value, NumberStyles.AllowThousands, CultureInfo, out var result) ? result : null;
		}
		
		private static TimeSpan? ParseTimeSpan(string value)
		{
			return TimeSpan.TryParseExact(value, TimeSpanFormats, CultureInfo, out var result) ? result : null;
		}
	}
}