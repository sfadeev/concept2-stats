using C2Stats.Entities;
using MediatR;

namespace C2Stats.Notifications
{
	public class ProfilesFileUpdated : INotification
	{
		public required ICollection<DbProfile> Updated { get; set; }
	}
}