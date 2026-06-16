using MassTransit;
using Microsoft.Extensions.Logging;
using QApplication.Caching;
using QContracts.CashingEvents;

namespace QInfrastructure.Consumers.Cache;

public class CompanyCacheResetConsumer : IConsumer<CompanyCacheResetEvent>
{
    private readonly ICacheService _cache;
    private readonly ILogger<CompanyCacheResetConsumer> _logger;

    public CompanyCacheResetConsumer(ICacheService cache, ILogger<CompanyCacheResetConsumer> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CompanyCacheResetEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Processing cache event for CompanyId {CompanyId}", evt.CompanyId);

        await _cache.HashRemoveAsync(CacheKeys.AllCompaniesKey);
        await _cache.RemoveAsync(CacheKeys.CompanyById(evt.CompanyId));

        _logger.LogInformation("Cache event processed for CompanyId {CompanyId}", evt.CompanyId);
    }
}