using MessagePack;
using QContracts.Enums;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class QueueInfoResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new QueueInfo
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            EmployeeId = 1,
            CustomerId = 1,
            CustomerName = "Test Customer Name",
            EmployeeName = "Test Employee Name",
            StartTime = DateTimeOffset.UtcNow.AddMinutes(10),
            EndTime = DateTimeOffset.UtcNow.AddMinutes(30),
            CurrentQueueStatus = CurrentQueueStatus.Pending,
            CancelReason = null,
            CreatedAt = DateTime.UtcNow,
            
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<QueueInfo>(bytes);
        
        deserializedResponse.Id.ShouldBe(originalResponse.Id);
        deserializedResponse.CompanyId.ShouldBe(originalResponse.CompanyId);
        deserializedResponse.BranchId.ShouldBe(originalResponse.BranchId);
        deserializedResponse.ServiceId.ShouldBe(originalResponse.ServiceId);
        deserializedResponse.EmployeeId.ShouldBe(originalResponse.EmployeeId);
        deserializedResponse.CustomerId.ShouldBe(originalResponse.CustomerId);
        deserializedResponse.CustomerName.ShouldBe(originalResponse.CustomerName);
        deserializedResponse.EmployeeName.ShouldBe(originalResponse.EmployeeName);
        deserializedResponse.StartTime.ShouldBe(originalResponse.StartTime);
        deserializedResponse.EndTime.ShouldBe(originalResponse.EndTime);
        deserializedResponse.CurrentQueueStatus.ShouldBe(originalResponse.CurrentQueueStatus);
        deserializedResponse.CancelReason.ShouldBe(originalResponse.CancelReason);
        deserializedResponse.CreatedAt.ShouldBe(originalResponse.CreatedAt);
    }
}