using C2Stats.Notifications;
using MediatR;

namespace C2Stats.Services
{
	public class ProfilesFileUpdatedHandler(IProfileDbStorage profileDbStorage) : INotificationHandler<ProfilesFileUpdated>
	{
		public async Task Handle(ProfilesFileUpdated notification, CancellationToken cancellationToken)
		{
			await profileDbStorage.Sync(notification.Updated, cancellationToken);
		}
	}
}