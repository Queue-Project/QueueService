using MessagePack;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class BlockedCustomerInfoResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new BlockedCustomerInfo()
        {
            BlockedId = 1,
            CustomerId = 1,
            CompanyId = 1,
            BannedUntil = DateTime.UtcNow.Date.AddMonths(1),
            DoesBanForever = false,
            Reason = "Test Reason,",
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<BlockedCustomerInfo>(bytes);
        
        deserializedResponse.BlockedId.ShouldBe(originalResponse.BlockedId);
        deserializedResponse.CustomerId.ShouldBe(originalResponse.CustomerId);
        deserializedResponse.CompanyId.ShouldBe(originalResponse.CompanyId);
        deserializedResponse.BannedUntil.ShouldBe(originalResponse.BannedUntil);
        deserializedResponse.DoesBanForever.ShouldBe(originalResponse.DoesBanForever);
        deserializedResponse.Reason.ShouldBe(originalResponse.Reason);
        deserializedResponse.CreatedAt.ShouldBe(originalResponse.CreatedAt);
    }
}