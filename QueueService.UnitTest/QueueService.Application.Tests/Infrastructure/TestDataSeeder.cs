using QDomain.Enums;
using QDomain.Models;

namespace QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;

public class TestDataSeeder
{
    public static ReviewEntity CreateReview()
    {
        return new ReviewEntity()
        {
            Id = 1,
            QueueId = 1,
            CustomerId = 1,
            Grade = 4,
            ReviewText = "Test Review Text",
            CreatedAt = DateTime.UtcNow,
            Queue = new QueueEntity
            {
                Id = 1,
                CompanyId = 1,
                BranchId = 1,
                ServiceId = 1,
                EmployeeId = 1,
                CustomerId = 1,
                StartTime = DateTimeOffset.UtcNow.Date.AddMinutes(10),
                EndTime = DateTimeOffset.UtcNow.Date.AddMinutes(40),
                Status = QueueStatus.Completed,
                CancelReason = null,
                IsStartingSoonNotified = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    public static List<ReviewEntity> CreateReviews()
    {
        return new List<ReviewEntity>
        {
            new ReviewEntity()
            {
                Id = 1,
                QueueId = 1,
                CustomerId = 1,
                Grade = 4,
                ReviewText = "Test Review Text",
                CreatedAt = DateTime.UtcNow,
                Queue = new QueueEntity
                {
                    Id = 1,
                    CompanyId = 1,
                    BranchId = 1,
                    ServiceId = 1,
                    EmployeeId = 1,
                    CustomerId = 1,
                    StartTime = DateTimeOffset.UtcNow.Date.AddMinutes(10),
                    EndTime = DateTimeOffset.UtcNow.Date.AddMinutes(40),
                    Status = QueueStatus.Completed,
                    CancelReason = null,
                    IsStartingSoonNotified = true,
                    CreatedAt = DateTime.UtcNow
                }
            },
            new ReviewEntity()
            {
                Id = 2,
                QueueId = 2,
                CustomerId = 1,
                Grade = 5,
                ReviewText = "Test Review Text",
                CreatedAt = DateTime.UtcNow,
                Queue = new QueueEntity
                {
                    Id = 2,
                    CompanyId = 1,
                    BranchId = 1,
                    ServiceId = 1,
                    EmployeeId = 1,
                    CustomerId = 1,
                    StartTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddMinutes(10),
                    EndTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddMinutes(40),
                    Status = QueueStatus.Completed,
                    CancelReason = null,
                    IsStartingSoonNotified = true,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
    }
    
    public static ComplaintEntity CreateComplaint()
    {
        return new ComplaintEntity()
        {
            Id = 1,
            QueueId = 1,
            CustomerId = 1,
            ComplaintText = "Test Complaint Text",
            ComplaintStatus = ComplaintStatus.Pending,
            CreatdAt = DateTime.UtcNow,
            Queue = new QueueEntity
            {
                Id = 1,
                CompanyId = 1,
                BranchId = 1,
                ServiceId = 1,
                EmployeeId = 1,
                CustomerId = 1,
                StartTime = DateTimeOffset.UtcNow.Date.AddMinutes(10),
                EndTime = DateTimeOffset.UtcNow.Date.AddMinutes(40),
                Status = QueueStatus.Completed,
                CancelReason = null,
                IsStartingSoonNotified = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }
    
    public static List<ComplaintEntity> CreateComplaints()
    {
        return new List<ComplaintEntity>()
        {
            new ComplaintEntity()
            {
                Id = 1,
                QueueId = 1,
                CustomerId = 1,
                ComplaintText = "Test Complaint Text",
                ComplaintStatus = ComplaintStatus.Pending,
                CreatdAt = DateTime.UtcNow,
                Queue = new QueueEntity
                {
                    Id = 1,
                    CompanyId = 1,
                    BranchId = 1,
                    ServiceId = 1,
                    EmployeeId = 1,
                    CustomerId = 1,
                    StartTime = DateTimeOffset.UtcNow.Date.AddMinutes(10),
                    EndTime = DateTimeOffset.UtcNow.Date.AddMinutes(40),
                    Status = QueueStatus.Completed,
                    CancelReason = null,
                    IsStartingSoonNotified = true,
                    CreatedAt = DateTime.UtcNow
                }
            },
            new ComplaintEntity()
            {
                Id = 2,
                QueueId = 2,
                CustomerId = 1,
                ComplaintText = "Test Complaint Text2",
                ComplaintStatus = ComplaintStatus.Pending,
                CreatdAt = DateTime.UtcNow,
                Queue = new QueueEntity
                {
                    Id = 2,
                    CompanyId = 1,
                    BranchId = 1,
                    ServiceId = 1,
                    EmployeeId = 1,
                    CustomerId = 1,
                    StartTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddMinutes(10),
                    EndTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddMinutes(40),
                    Status = QueueStatus.Completed,
                    CancelReason = null,
                    IsStartingSoonNotified = true,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
    }

    public static QueueEntity CreateQueue()
    {
        return new QueueEntity
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            EmployeeId = 1,
            CustomerId = 1,
            StartTime = DateTimeOffset.UtcNow.Date.AddMinutes(10),
            EndTime = new DateTimeOffset(new DateTime(2026,06,22, 9,50,00)),
            Status = QueueStatus.Pending,
            CancelReason = null,
            IsStartingSoonNotified = true,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    public static QueueEntity CreateQueueWithStartTime()
    {
        return new QueueEntity
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            EmployeeId = 1,
            CustomerId = 1,
            StartTime = DateTimeOffset.UtcNow.DateTime.AddHours(15).AddMinutes(10),
            EndTime = DateTimeOffset.UtcNow.DateTime.AddHours(15).AddMinutes(50),
            Status = QueueStatus.Pending,
            CancelReason = null,
            IsStartingSoonNotified = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}