using MessagePack;
using QContracts.Enums;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class ComplaintInfoResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new ComplaintInfo
        {
            Id = 1,
            CustomerId = 1,
            QueueId = 1,
            EmployeeId = 1,
            ComplaintText = "Test Complaint Test",
            ResponseText = null,
            Status = CurrentComplaintStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<ComplaintInfo>(bytes);
        
        deserializedResponse.Id.ShouldBe(originalResponse.Id);
        deserializedResponse.CustomerId.ShouldBe(originalResponse.CustomerId);
        deserializedResponse.QueueId.ShouldBe(originalResponse.QueueId);
        deserializedResponse.EmployeeId.ShouldBe(originalResponse.EmployeeId);
        deserializedResponse.ComplaintText.ShouldBe(originalResponse.ComplaintText);
        deserializedResponse.ResponseText.ShouldBe(originalResponse.ResponseText);
        deserializedResponse.Status.ShouldBe(originalResponse.Status);
        deserializedResponse.CreatedAt.ShouldBe(originalResponse.CreatedAt);
    }
}