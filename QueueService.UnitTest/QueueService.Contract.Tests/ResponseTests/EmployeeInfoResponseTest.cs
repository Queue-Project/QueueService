using MessagePack;
using QContracts.Responses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Contract.Tests.ResponseTests;

public class EmployeeInfoResponseTest
{
    [Fact]
    public void QueueConfigurationResponse_ShouldSerializeAndDeserializeCorrectly()
    {
        var originalResponse = new EmployeeInfo()
        {
            CompanyId = 1,
            BranchId = 1,
            CompanyServiceId = 1,
            EmployeeId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            CreatedAt = DateTime.UtcNow
        };

        var bytes = MessagePackSerializer.Serialize(originalResponse);
        var deserializedResponse = MessagePackSerializer.Deserialize<EmployeeInfo>(bytes);
        
        deserializedResponse.CompanyId.ShouldBe(originalResponse.CompanyId);
        deserializedResponse.BranchId.ShouldBe(originalResponse.BranchId);
        deserializedResponse.CompanyServiceId.ShouldBe(originalResponse.CompanyServiceId);
        deserializedResponse.EmployeeId.ShouldBe(originalResponse.EmployeeId);
        deserializedResponse.FirstName.ShouldBe(originalResponse.FirstName);
        deserializedResponse.LastName.ShouldBe(originalResponse.LastName);
        deserializedResponse.Position.ShouldBe(originalResponse.Position);
        deserializedResponse.CreatedAt.ShouldBe(originalResponse.CreatedAt);
    }
}