using MessagePack;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class CustomerInfoResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new CustomerInfo()
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<CustomerInfo>(bytes);
        
        deserializedResponse.CustomerId.ShouldBe(originalResponse.CustomerId);
        deserializedResponse.FirstName.ShouldBe(originalResponse.FirstName);
        deserializedResponse.LastName.ShouldBe(originalResponse.LastName);
        deserializedResponse.CreatedAt.ShouldBe(originalResponse.CreatedAt);
    }
}