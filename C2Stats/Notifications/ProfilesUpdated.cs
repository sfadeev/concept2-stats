using C2Stats.Entities;
using MediatR;

namespace C2Stats.Notifications
{
	public class ProfilesUpdated : INotification
	{
		public required ICollection<DbProfile> Updated { get; set; }
	}
}