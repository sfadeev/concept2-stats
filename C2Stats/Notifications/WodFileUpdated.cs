using C2Stats.Models;
using MediatR;

namespace C2Stats.Notifications
{
	public class WodFileUpdated : INotification
	{
		public required WodResult Wod { get; set; }
	}
}