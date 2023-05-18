using Sanet.Bots.Telegram.Models;

namespace Sanet.Bots.Telegram.Services;

public interface IDatabaseService
{
    Task<bool> SaveSubscription(SubscriptionEntity item);
    Task<IEnumerable<SubscriptionEntity>> GetSubscriptionsForGroup(ulong groupId);
}