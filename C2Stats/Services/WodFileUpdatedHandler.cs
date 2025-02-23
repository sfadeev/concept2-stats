using C2Stats.Notifications;
using MediatR;

namespace C2Stats.Services
{
	public class WodFileUpdatedHandler(IProfileFileStorage profileFileStorage, IWodDbStorage wodDbStorage) : INotificationHandler<WodFileUpdated>
	{
		public async Task Handle(WodFileUpdated notification, CancellationToken cancellationToken)
		{
			// update profiles.json from wod results
			profileFileStorage.UpdateFrom(notification.Wod);
			
			// sync wod results to db
			await wodDbStorage.Sync(notification.Wod, cancellationToken);
		}
	}
}