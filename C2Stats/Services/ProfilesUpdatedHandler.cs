using C2Stats.Notifications;
using MediatR;

namespace C2Stats.Services
{
	public class ProfilesUpdatedHandler(IProfileDbStorage profileDbStorage) : INotificationHandler<ProfilesUpdated>
	{
		public async Task Handle(ProfilesUpdated notification, CancellationToken cancellationToken)
		{
			await profileDbStorage.Sync(notification.Updated, cancellationToken);
		}
	}
}