using System.Net;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QApplication.Interfaces.Data;
using QContracts.Enums;
using QContracts.Interfaces;
using QContracts.Responses;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.EmployeeRequests;

namespace QApplication.Services;

public class QueueService : ServiceBase<IQueueService>, IQueueService
{
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly ILogger<QueueService> _logger;
    private readonly IUserService _userService;
    private readonly IPersonNameProvider _personName;

    public QueueService(IQueueApplicationDbContext dbContext, ILogger<QueueService> logger, IUserService userService, IPersonNameProvider personName)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _personName = personName;
    }


    public async UnaryResult<QueueInfo> GetQueueByIdAsync(int queueId)
    {
        _logger.LogInformation("Getting queue with Id: {QueueId}", queueId);

        var queue = await _dbContext.Queues
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == queueId);

        if (queue == null)
        {
            _logger.LogWarning("Queue with Id: {QueueId} not found", queueId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Queue with Id {queueId} not found");
        }

        var customerName = await _personName.GetCustomerNameAsync(queue.CustomerId);
        var employeeName = await  _personName.GetEmployeeNameAsync(queue.EmployeeId);

        return new QueueInfo
        {
            Id = queue.Id,
            CompanyId = queue.CompanyId,
            BranchId = queue.BranchId,
            ServiceId = queue.ServiceId,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            CustomerName = customerName,
            EmployeeName = employeeName,
            StartTime = queue.StartTime,
            EndTime = queue.EndTime,
            CurrentQueueStatus = (CurrentQueueStatus)queue.Status,
            CancelReason = queue.CancelReason,
            CreatedAt = queue.CreatedAt
        };
    }

    public async UnaryResult<List<QueueInfo>> GetCustomerQueuesAsync(int customerId)
    {
        _logger.LogInformation("Getting queues with customer Id: {customerId}", customerId);

        var customer = await _userService.GetCustomerById(new CustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = customerId
        });

        if (!customer.IsValid)
        {
            _logger.LogWarning("Customer with Id {CustomerId} not found", customerId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Customer with Id {customerId} not found");
        }


        var customerQueues = await _dbContext.Queues
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();

        if (!customerQueues.Any())
        {
            _logger.LogWarning("Not found queues for this customer");
            return [];
        }

        var response = new List<QueueInfo>();
        foreach (var queue in customerQueues)
        {
            var employeeName = await _personName.GetEmployeeNameAsync(queue.EmployeeId);
            response.Add(new QueueInfo
            {
                Id = queue.Id,
                CompanyId = queue.CompanyId,
                BranchId = queue.BranchId,
                ServiceId = queue.ServiceId,
                CustomerId = queue.CustomerId,
                EmployeeId = queue.EmployeeId,
                CustomerName = $"{customer.FirstName} {customer.LastName}",
                EmployeeName = employeeName,
                StartTime = queue.StartTime,
                EndTime = queue.EndTime,
                CurrentQueueStatus = (CurrentQueueStatus)queue.Status,
                CancelReason = queue.CancelReason,
                CreatedAt = queue.CreatedAt
            });
        }

        return response;
    }

    public async UnaryResult<List<QueueInfo>> GetEmployeeQueuesAsync(int employeeId)
    {
        _logger.LogInformation("Getting queues with employee Id: {employeeId}", employeeId);


        var employee = await _userService.GetEmployeeById(new EmployeeByIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = employeeId,
        });

        if (!employee.IsValid)
        {
            _logger.LogWarning("Employee with Id {employeeId} not found", employeeId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Employee with Id {employeeId} not found");
        }


        var employeeQueues = await _dbContext.Queues
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId)
            .ToListAsync();

        if (!employeeQueues.Any())
        {
            _logger.LogWarning("Not found queues for this employee");
            return [];
        }

        var response = new List<QueueInfo>();
        foreach (var queue in employeeQueues)
        {
            var customerName = await _personName.GetCustomerNameAsync(queue.EmployeeId);
            response.Add(new QueueInfo
            {
                Id = queue.Id,
                CompanyId = queue.CompanyId,
                BranchId = queue.BranchId,
                ServiceId = queue.ServiceId,
                CustomerId = queue.CustomerId,
                EmployeeName = employee.FirstName,
                CustomerName = customerName,
                StartTime = queue.StartTime,
                EndTime = queue.EndTime,
                CurrentQueueStatus = (CurrentQueueStatus)queue.Status,
                CancelReason = queue.CancelReason,
                CreatedAt = queue.CreatedAt
            });
        }


        return response;
    }

    public async UnaryResult<List<QueueInfo>> GetBranchQueuesAsync(int branchId)
    {
        _logger.LogInformation("Getting queues for branch Id: {BranchId}", branchId);

        var branchQueues = await _dbContext.Queues
            .AsNoTracking()
            .Where(s => s.BranchId == branchId)
            .ToListAsync();

        if (!branchQueues.Any())
        {
            _logger.LogWarning("No queues found for branch {BranchId}", branchId);
            return new List<QueueInfo>();
        }

        var response = new List<QueueInfo>();
        foreach (var queue in branchQueues)
        {
            var customerName = await _personName.GetCustomerNameAsync(queue.CustomerId);
            var employeeName = await _personName.GetEmployeeNameAsync(queue.EmployeeId);

            response.Add(new QueueInfo
            {
                Id = queue.Id,
                CompanyId = queue.CompanyId,
                BranchId = queue.BranchId,
                ServiceId = queue.ServiceId,
                CustomerId = queue.CustomerId,
                EmployeeId = queue.EmployeeId,
                CustomerName = customerName,
                EmployeeName = employeeName,
                StartTime = queue.StartTime,
                EndTime = queue.EndTime,
                CurrentQueueStatus = (CurrentQueueStatus)queue.Status,
                CancelReason = queue.CancelReason,
                CreatedAt = queue.CreatedAt
            });
        }

        return response;
    }

    public async UnaryResult<List<QueueInfo>> GetCompanyQueuesAsync(int companyId)
    {
        _logger.LogInformation("Getting queues for company Id: {CompanyId}", companyId);

        var companyQueues = await _dbContext.Queues
            .AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .ToListAsync();

        if (!companyQueues.Any())
        {
            _logger.LogWarning("No queues found for company {CompanyId}", companyId);
            return new List<QueueInfo>();
        }

        var response = new List<QueueInfo>();
        foreach (var queue in companyQueues)
        {
            var customerName = await _personName.GetCustomerNameAsync(queue.CustomerId);
            var employeeName = await _personName.GetEmployeeNameAsync(queue.EmployeeId);

            response.Add(new QueueInfo
            {
                Id = queue.Id,
                CompanyId = queue.CompanyId,
                BranchId = queue.BranchId,
                ServiceId = queue.ServiceId,
                CustomerId = queue.CustomerId,
                EmployeeId = queue.EmployeeId,
                CustomerName = customerName,
                EmployeeName = employeeName,
                StartTime = queue.StartTime,
                EndTime = queue.EndTime,
                CurrentQueueStatus = (CurrentQueueStatus)queue.Status,
                CancelReason = queue.CancelReason,
                CreatedAt = queue.CreatedAt
            });
        }

        return response;
    }

    public async UnaryResult<List<QueueInfo>> GetServiceQueuesAsync(int serviceId)
    {
        _logger.LogInformation("Getting queues for service Id: {ServiceId}", serviceId);

        var serviceQueues = await _dbContext.Queues
            .AsNoTracking()
            .Where(s => s.ServiceId == serviceId)
            .ToListAsync();

        if (!serviceQueues.Any())
        {
            _logger.LogWarning("No queues found for service {ServiceId}", serviceId);
            return new List<QueueInfo>();
        }

        var response = new List<QueueInfo>();
        foreach (var queue in serviceQueues)
        {
            var customerName = await _personName.GetCustomerNameAsync(queue.CustomerId);
            var employeeName = await _personName.GetEmployeeNameAsync(queue.EmployeeId);

            response.Add(new QueueInfo
            {
                Id = queue.Id,
                CompanyId = queue.CompanyId,
                BranchId = queue.BranchId,
                ServiceId = queue.ServiceId,
                CustomerId = queue.CustomerId,
                EmployeeId = queue.EmployeeId,
                CustomerName = customerName,
                EmployeeName = employeeName,
                StartTime = queue.StartTime,
                EndTime = queue.EndTime,
                CurrentQueueStatus = (CurrentQueueStatus)queue.Status,
                CancelReason = queue.CancelReason,
                CreatedAt = queue.CreatedAt
            });
        }

        return response;
    }

    public async UnaryResult<ReviewInfo> GetQueueReviewAsync(int queueId)
    {
        _logger.LogInformation("Getting review for queue Id {queueId}", queueId);

        var queue = await _dbContext.Queues
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == queueId);

        if (queue == null)
        {
            _logger.LogWarning("Queue with Id {queueId} not found", queueId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Queue with Id {queueId} not found");
        }

        var queueReview = await _dbContext.Reviews
            .FirstOrDefaultAsync(s => s.QueueId == queueId);

        if (queueReview == null)
        {
            _logger.LogInformation("Not found review for this queue");
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, "Not found review for this request");
        }

        var response = new ReviewInfo
        {
            Id = queueReview.Id,
            QueueId = queueReview.QueueId,
            EmployeeId = queueReview.Queue.EmployeeId,
            CustomerId = queueReview.CustomerId,
            Grade = queueReview.Grade,
            ReviewText = queueReview.ReviewText,
            CreatedAt = queueReview.CreatedAt
        };

        return response;
    }

    public async UnaryResult<List<ReviewInfo>> GetEmployeeReviewsAsync(int employeeId)
    {
        _logger.LogInformation("Getting reviews for employee Id: {employeeId}", employeeId);

        var employee = await _userService.GetEmployeeById(new EmployeeByIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = employeeId
        });

        if (!employee.IsValid)
        {
            _logger.LogWarning("Employee with Id {employeeId} not found", employeeId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Employee with Id {employeeId} not found");
        }


        var employeeReviews = await _dbContext.Reviews
            .AsNoTracking()
            .Include(s => s.Queue)
            .Where(s => s.Queue.EmployeeId == employeeId)
            .ToListAsync();

        if (!employeeReviews.Any())
        {
            _logger.LogWarning("Not found any review for this customer");
            return [];
        }

        var response = employeeReviews.Select(review => new ReviewInfo()
        {
            Id = review.Id,
            QueueId = review.QueueId,
            EmployeeId = review.Queue.EmployeeId,
            CustomerId = review.CustomerId,
            Grade = review.Grade,
            ReviewText = review.ReviewText,
            CreatedAt = review.CreatedAt
        }).ToList();

        return response;
    }

    public async UnaryResult<List<ReviewInfo>> GetCustomerReviewsAsync(int customerId)
    {
        _logger.LogInformation("Getting reviews for customer Id: {customerId}", customerId);

        var customer = await _userService.GetCustomerById(new CustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = customerId
        });

        if (!customer.IsValid)
        {
            _logger.LogWarning("Customer with Id {customerId} not found", customerId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Customer with Id {customerId} not found");
        }

        var customerReviews = await _dbContext.Reviews
            .AsNoTracking()
            .Include(s => s.Queue)
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();

        if (!customerReviews.Any())
        {
            _logger.LogWarning("Not found any review for this customer");
            return [];
        }

        var response = customerReviews.Select(review => new ReviewInfo()
        {
            Id = review.Id,
            QueueId = review.QueueId,
            EmployeeId = review.Queue.EmployeeId,
            CustomerId = review.CustomerId,
            Grade = review.Grade,
            ReviewText = review.ReviewText,
            CreatedAt = review.CreatedAt
        }).ToList();

        return response;
    }

    public async UnaryResult<List<ReviewInfo>> GetCompanyReviewsAsync(int companyId)
    {
        _logger.LogInformation("Getting reviews for company Id: {companyId}", companyId);

        var companyReviews = await _dbContext.Reviews
            .AsNoTracking()
            .Include(s => s.Queue)
            .Where(s => s.Queue.CompanyId == companyId)
            .ToListAsync();

        if (!companyReviews.Any())
        {
            _logger.LogWarning("Not found any review for this company");
            return [];
        }

        var response = companyReviews.Select(review => new ReviewInfo()
        {
            Id = review.Id,
            QueueId = review.QueueId,
            EmployeeId = review.Queue.EmployeeId,
            CustomerId = review.CustomerId,
            Grade = review.Grade,
            ReviewText = review.ReviewText,
            CreatedAt = review.CreatedAt
        }).ToList();

        return response;
    }

    public async UnaryResult<ComplaintInfo> GetQueueComplaintAsync(int queueId)
    {
        _logger.LogInformation("Getting complaint for queue Id {queueId}", queueId);

        var queue = await _dbContext.Queues
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == queueId);

        if (queue == null)
        {
            _logger.LogWarning("Queue with Id {queueId} not found", queueId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Queue with Id {queueId} not found");
        }

        var queueComplaint = await _dbContext.Complaints
            .FirstOrDefaultAsync(s => s.QueueId == queueId);

        if (queueComplaint == null)
        {
            _logger.LogInformation("Not found any complaint for this queue");
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, "Not found any complaint for this queue");
        }

        var response = new ComplaintInfo()
        {
            Id = queueComplaint.Id,
            QueueId = queueComplaint.QueueId,
            EmployeeId = queueComplaint.Queue.EmployeeId,
            CustomerId = queueComplaint.CustomerId,
            ComplaintText = queueComplaint.ComplaintText,
            ResponseText = queueComplaint.ResponseText,
            Status = (CurrentComplaintStatus)queueComplaint.ComplaintStatus,
            CreatedAt = queueComplaint.CreatdAt
        };

        return response;
    }

    public async UnaryResult<List<ComplaintInfo>> GetEmployeeComplaintsAsync(int employeeId)
    {
        _logger.LogInformation("Getting complaints for employee Id: {employeeId}", employeeId);


        var employee = await _userService.GetEmployeeById(new EmployeeByIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = employeeId
        });

        if (!employee.IsValid)
        {
            _logger.LogWarning("Employee with Id {employeeId} not found", employeeId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Employee with Id {employeeId} not found");
        }

        var employeeComplaints = await _dbContext.Complaints
            .AsNoTracking()
            .Include(s => s.Queue)
            .Where(s => s.Queue.EmployeeId == employeeId)
            .ToListAsync();

        if (!employeeComplaints.Any())
        {
            _logger.LogWarning("Not found any customer complaints");
            return [];
        }

        var response = employeeComplaints.Select(complaint => new ComplaintInfo()
        {
            Id = complaint.Id,
            QueueId = complaint.QueueId,
            EmployeeId = complaint.Queue.EmployeeId,
            CustomerId = complaint.CustomerId,
            ComplaintText = complaint.ComplaintText,
            ResponseText = complaint.ResponseText,
            Status = (CurrentComplaintStatus)complaint.ComplaintStatus,
            CreatedAt = complaint.CreatdAt
        }).ToList();

        return response;
    }

    public async UnaryResult<List<ComplaintInfo>> GetCustomerComplaintsAsync(int customerId)
    {
        _logger.LogInformation("Getting complaints for customer Id: {customerId}", customerId);

        var customer = await _userService.GetCustomerById(new CustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = customerId
        });

        if (!customer.IsValid)
        {
            _logger.LogWarning("Customer with Id {customerId} not found", customerId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Customer with Id {customerId} not found");
        }


        var customerComplaints = await _dbContext.Complaints
            .AsNoTracking()
            .Include(s => s.Queue)
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();

        if (!customerComplaints.Any())
        {
            _logger.LogWarning("Not found any customer complaints");
            return [];
        }

        var response = customerComplaints.Select(complaint => new ComplaintInfo()
        {
            Id = complaint.Id,
            QueueId = complaint.QueueId,
            EmployeeId = complaint.Queue.EmployeeId,
            CustomerId = complaint.CustomerId,
            ComplaintText = complaint.ComplaintText,
            ResponseText = complaint.ResponseText,
            Status = (CurrentComplaintStatus)complaint.ComplaintStatus,
            CreatedAt = complaint.CreatdAt
        }).ToList();

        return response;
    }

    public async UnaryResult<List<ComplaintInfo>> GetCompanyComplaintsAsync(int companyId)
    {
        _logger.LogInformation("Getting complaints for company Id: {companyId}", companyId);

        var companyComplaints = await _dbContext.Complaints
            .AsNoTracking()
            .Include(s => s.Queue)
            .Where(s => s.Queue.CompanyId == companyId)
            .ToListAsync();

        if (!companyComplaints.Any())
        {
            _logger.LogWarning("Not found any company complaints");
            return [];
        }

        var response = companyComplaints.Select(complaint => new ComplaintInfo()
        {
            Id = complaint.Id,
            QueueId = complaint.QueueId,
            EmployeeId = complaint.Queue.EmployeeId,
            CustomerId = complaint.CustomerId,
            ComplaintText = complaint.ComplaintText,
            ResponseText = complaint.ResponseText,
            Status = (CurrentComplaintStatus)complaint.ComplaintStatus,
            CreatedAt = complaint.CreatdAt
        }).ToList();

        return response;
    }

    public async UnaryResult<List<CustomerInfo>> GetAllCompanyCustomers(int companyId)
    {
        _logger.LogInformation("Getting all customers for company Id: {CompanyId}", companyId);

        var customerIds = await _dbContext.Queues
            .Where(q => q.CompanyId == companyId)
            .Select(q => q.CustomerId)
            .Distinct()
            .ToListAsync();

        if (!customerIds.Any())
        {
            _logger.LogWarning("No customers found for company {CompanyId}", companyId);
            return new List<CustomerInfo>();
        }

        var response = new List<CustomerInfo>();
        foreach (var customerId in customerIds)
        {
            var customer = await _userService.GetCustomerById(new CustomerByIdRequest
            {
                RequestId = Guid.NewGuid(),
                CustomerId = customerId
            });

            if (customer.IsValid)
            {
                response.Add(new CustomerInfo
                {
                    CustomerId = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    CreatedAt = customer.CreatedAt
                });
            }
        }

        return response;
    }
}