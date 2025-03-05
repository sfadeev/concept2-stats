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
		private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;
		
		private static readonly string[] TimeSpanFormats = [ @"m\:ss\.f" ];

		public WodResult Parse(string html)
		{
			var result = new WodResult();

			var doc = new HtmlDocument();
			
			doc.LoadHtml(html);
			
			var stats = doc.DocumentNode?.SelectNodes("//section[@class='sidebar']//div[@class='stat ']");
			
			if (stats != null)
			{
				foreach (var stat in stats)
				{
					var statFigure = stat.SelectSingleNode("//div[@class='stat__figure']");
					var statName = stat.SelectSingleNode("//div[@class='stat__name']");

					if (statFigure != null && statName != null && statName.InnerText.Contains("Total people"))
					{
						result.TotalCount = ParseNumber(statFigure.InnerText);
						
						break;
					}
				}	
			}
			
			var content = doc.DocumentNode?.SelectSingleNode("//section[@class='content']");
			
			if (content != null)
			{
				result.Name = content.SelectSingleNode("h3")?.InnerText;
				result.Description = content.SelectSingleNode("p/strong")?.InnerText;
				
				var paging = content.SelectSingleNode("//ul[@class='pagination']");

				if (paging != null)
				{
					var maxPageNo = 0;
					
					foreach (var page in paging.SelectNodes("li"))
					{
						var pageNo = ParseNumber(page.InnerText);
						
						if (pageNo != null) maxPageNo = pageNo.Value;
					}

					result.TotalPageCount = maxPageNo;
				}
				
				if (content.InnerText.Contains("Sorry, no results were found."))
				{
					result.Has404Error = true;
				
					return result;
				}
				
				if (content.InnerText.Contains("Sorry, it looks like something has gone wrong!"))
				{
					result.Has500Error = true;
				
					return result;
				}
				
				var columns = content.SelectNodes("table/thead/tr/th");
				var rows = content.SelectNodes("table/tbody/tr");

				int? posIndex = null,
					nameIndex = null,
					ageIndex = null,
					locationIndex = null,
					countryIndex = null,
					clubIndex = null,
					resultIndex = null,
					paceIndex = null;
				
				if (columns != null)
				{
					for (var i = 0; i < columns.Count; i++)
					{
						var column = columns[i];
						
						if (column.InnerText == "Pos.") posIndex = i;
						if (column.InnerText == "Name") nameIndex = i;
						if (column.InnerText == "Age") ageIndex = i;
						if (column.InnerText == "Location") locationIndex = i;
						if (column.InnerText == "Country") countryIndex = i;
						if (column.InnerText == "Club/Affiliation") clubIndex = i;
						if (column.InnerText == "Result") resultIndex = i;
						if (column.InnerText == "Pace") paceIndex = i;
					}
				}

				if (rows != null)
				{
					foreach (var row in rows)
					{
						var item = new WodResultItem();

						var cells = row.SelectNodes("td");

						for (var i = 0; i < cells.Count; i++)
						{
							var cell = cells[i];

							if (i == posIndex)
							{
								item.Position = ParseNumber(cell.InnerText);
							}
							else if (i == nameIndex)
							{
								var anchor = cell.SelectSingleNode("a");

								if (anchor != null)
								{
									item.Name = ParseString(anchor.InnerText);

									var href = anchor.Attributes["href"]?.Value;

									if (href != null)
									{
										var idIndex = href.LastIndexOf('/');

										if (idIndex > 0) item.Id = ParseNumber(href[(idIndex + 1)..]);
									}
								}
							}
							else if (i == ageIndex)
							{
								item.Age = ParseNumber(cell.InnerText);
							}
							else if (i == locationIndex)
							{
								item.Location = ParseString(cell.InnerText);
							}
							else if (i == countryIndex)
							{
								item.Country = ParseString(cell.InnerText);
							}
							else if (i == clubIndex)
							{
								item.Affiliation = ParseString(cell.InnerText);
							}
							else if (i == resultIndex)
							{
								if (cell.InnerText.EndsWith('m'))
								{
									item.ResultMeters = ParseNumber(cell.InnerText[..^1]);
								}
								else if (cell.InnerText.Contains(':'))
								{
									item.ResultTime = ParseTimeSpan(cell.InnerText);
								}
								else
								{
									item.ResultMeters = ParseNumber(cell.InnerText);
								}
							}
							else if (i == paceIndex)
							{
								item.Pace = ParseTimeSpan(cell.InnerText);
							}
						}

						result.Items.Add(item);
					}
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
			return int.TryParse(value.Trim(), NumberStyles.AllowThousands, CultureInfo, out var result) ? result : null;
		}
		
		private static TimeSpan? ParseTimeSpan(string value)
		{
			return TimeSpan.TryParseExact(value, TimeSpanFormats, CultureInfo, out var result) ? result : null;
		}
	}
}