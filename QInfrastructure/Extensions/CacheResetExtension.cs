using QApplication.Caching;

namespace QInfrastructure.Extensions;

public static class CacheResetExtensions
{
    public static async Task ResetCacheAsync(
        this ICacheService cacheService,
        int queueId,
        int customerId,
        int employeeId)
    {
        await Task.WhenAll(
            cacheService.HashRemoveAsync(CacheKeys.AllQueuesHashKey),
            cacheService.HashRemoveAsync(CacheKeys.CustomerQueuesHashKey(customerId)),
            cacheService.RemoveAsync(CacheKeys.QueueId(queueId)),
            cacheService.RemoveAsync(CacheKeys.EmployeeId(employeeId)));
    }
}

