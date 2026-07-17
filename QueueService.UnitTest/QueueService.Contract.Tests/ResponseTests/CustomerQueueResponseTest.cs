using MessagePack;
using QContracts.Events.Enums;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class CustomerQueueResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new CustomerQueueResponse()
        {
           QueueId = 1,
           CompanyId = 1,
           BranchId = 1,
           ServiceId = 1,
           EmployeeId = 1,
           StartTime = DateTimeOffset.UtcNow,
           Status = UpdatedQueueStatus.Pending
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<CustomerQueueResponse>(bytes);
        
        
        deserializedResponse.QueueId.ShouldBe(originalResponse.QueueId);
        deserializedResponse.CompanyId.ShouldBe(originalResponse.CompanyId);
        deserializedResponse.BranchId.ShouldBe(originalResponse.BranchId);
        deserializedResponse.ServiceId.ShouldBe(originalResponse.ServiceId);
        deserializedResponse.EmployeeId.ShouldBe(originalResponse.EmployeeId);
        deserializedResponse.StartTime.ShouldBe(originalResponse.StartTime);
        deserializedResponse.Status.ShouldBe(originalResponse.Status);
        
    }
}