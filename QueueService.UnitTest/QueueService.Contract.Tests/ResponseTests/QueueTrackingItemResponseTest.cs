using MessagePack;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class QueueTrackingItemResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new QueueTrackingItemResponse
        {
            CompanyServiceId = 1,
            QueueId = 2,
            StartTime = DateTimeOffset.UtcNow
        };


        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<QueueTrackingItemResponse>(bytes);


        deserializedResponse.QueueId.ShouldBe(originalResponse.QueueId);
        deserializedResponse.CompanyServiceId.ShouldBe(originalResponse.CompanyServiceId);
        deserializedResponse.StartTime.ShouldBe(originalResponse.StartTime);
    }
}