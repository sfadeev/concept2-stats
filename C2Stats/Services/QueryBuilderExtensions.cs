using Microsoft.AspNetCore.Http.Extensions;

namespace C2Stats.Services
{
	public static class QueryBuilderExtensions
	{
		public static QueryBuilder Add(this QueryBuilder queryBuilder, string key, int value)
		{
			queryBuilder.Add(key, value.ToString());
				
			return queryBuilder;
		}
		
		public static QueryBuilder AddIfExists(this QueryBuilder queryBuilder, string key, int? value)
		{
			if (value != null) queryBuilder.Add(key, value.Value.ToString());
				
			return queryBuilder;
		}
	}
}