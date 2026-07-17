using MessagePack;
using QContracts.Events.Enums;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class QueueTrackingDataResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new QueueTrackingDataResponse()
        {
            QueueId = 1,
            EmployeeId = 1,
            StartTime = DateTimeOffset.UtcNow,
            Status = UpdatedQueueStatus.Pending,
            CompanyServiceId = 1,
            QueuesAhead = new List<QueueTrackingItemResponse>()
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<QueueTrackingDataResponse>(bytes);


        deserializedResponse.QueueId.ShouldBe(originalResponse.QueueId);
        deserializedResponse.CompanyServiceId.ShouldBe(originalResponse.CompanyServiceId);
        deserializedResponse.EmployeeId.ShouldBe(originalResponse.EmployeeId);
        deserializedResponse.StartTime.ShouldBe(originalResponse.StartTime);
        deserializedResponse.Status.ShouldBe(originalResponse.Status);
        deserializedResponse.QueuesAhead.ShouldBe(originalResponse.QueuesAhead);
    }
}