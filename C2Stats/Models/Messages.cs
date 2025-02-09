namespace C2Stats.Models
{
	public static class Messages
	{
		public static string ServiceStarted(string? baseUrl)
		{
			return "Service started 👍\n" + baseUrl;
		}

		public static string ServiceStopped()
		{
			return "Service stopped 👎";
		}
	}
}