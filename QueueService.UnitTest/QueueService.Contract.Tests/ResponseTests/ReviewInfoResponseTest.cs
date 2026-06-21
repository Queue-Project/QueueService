using MessagePack;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class ReviewInfoResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new ReviewInfo()
        {
            Id = 1,
            EmployeeId = 1,
            CustomerId = 1,
            QueueId = 1,
            Grade = 5,
            ReviewText = "Test Review Text",
            CreatedAt = DateTime.UtcNow,
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<ReviewInfo>(bytes);

        deserializedResponse.Id.ShouldBe(originalResponse.Id);
        deserializedResponse.EmployeeId.ShouldBe(originalResponse.EmployeeId);
        deserializedResponse.CustomerId.ShouldBe(originalResponse.CustomerId);
        deserializedResponse.QueueId.ShouldBe(originalResponse.QueueId);
        deserializedResponse.Grade.ShouldBe(originalResponse.Grade);
        deserializedResponse.ReviewText.ShouldBe(originalResponse.ReviewText);
        deserializedResponse.CreatedAt.ShouldBe(originalResponse.CreatedAt);
    }
}