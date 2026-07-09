using System.Net;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Exceptions;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QContracts.Events;
using QContracts.Events.Enums;
using QDomain.Enums;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Queues.Commands.CreateQueue;

public class CreateQueueCommandHandler : IRequestHandler<CreateQueueCommand, AddQueueResponseModel>
{
    private readonly ILogger<CreateQueueCommandHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IBranchService _branchService;
    private readonly IUserService _userService;

    public CreateQueueCommandHandler(ILogger<CreateQueueCommandHandler> logger, IQueueApplicationDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        IHttpContextAccessor contextAccessor,
        IBranchService branchService, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _contextAccessor = contextAccessor;
        _branchService = branchService;
        _userService = userService;
    }

    public async Task<AddQueueResponseModel> Handle(CreateQueueCommand request, CancellationToken cancellationToken)
    {
        
        
        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var currentCustomer = await _userService.GetCurrentCustomer(new CurrentUserRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId,
        });
        
        if (!currentCustomer.IsValid)
        {
            _logger.LogWarning("User is not a customer");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                $"User is not a customer");
        }
        
        var customerId = currentCustomer.CustomerId;

        var currentUser = await _userService.GetUserEmailByCustomerId(new GetUserEmailByCustomerIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = customerId,
        });
        
        if (!currentUser.IsValid)
        {
            _logger.LogWarning("User not found for this customer");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                $"User not found for this customer");
        }
        
      
        var userEmail = currentUser.EmailAddress;

        _logger.LogInformation("Adding new queue for EmployeeId {id}", request.EmployeeId);


        var validationResponse = await _branchService.ValidateQueueCreationAsync(
            new QueueCreationValidationRequest
            {
                RequestId = Guid.NewGuid(),
                BranchId = request.BranchId,
                RequestedStartTime = request.StartTime
            });

        if (!validationResponse.IsValid)
        {
            _logger.LogWarning("Branch validation failed: {ErrorMessage}", validationResponse.ErrorMessage);
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, validationResponse.ErrorMessage!);
        }

        
        var ticketsToday = await _dbContext.Queues
            .CountAsync(q => q.BranchId == request.BranchId &&
                             q.StartTime.Date == request.StartTime.Date &&
                             q.Status != QueueStatus.CancelledByEmployee &&
                             q.Status != QueueStatus.CancelledByCustomer,
                cancellationToken);

        if (ticketsToday >= validationResponse.MaxTicketsPerDay)
        {
            _logger.LogWarning("Daily ticket limit reached for Branch {BranchId}. Today: {TicketsToday}/{MaxTickets}",
                request.BranchId, ticketsToday, validationResponse.MaxTicketsPerDay);

            throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                $"Maximum tickets for today ({validationResponse.MaxTicketsPerDay}) has been reached");
        }

        _logger.LogInformation("Branch validation passed. Tickets today: {TicketsToday}/{MaxTickets}",
            ticketsToday, validationResponse.MaxTicketsPerDay);


        var companyResult = await _branchService.CheckCompanyId(new CompanyRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            RequestedAt = DateTimeOffset.UtcNow
        });

        if (!companyResult.IsValid)
        {
            _logger.LogInformation("Company with Id {CompanyId} not found", request.CompanyId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                companyResult.ErrorMessage ?? "Company not found");
        }

        var branchResult = await _branchService.CheckBranchId(new BranchRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            RequestedAt = DateTimeOffset.UtcNow
        });

        if (!branchResult.IsValid)
        {
            _logger.LogInformation("Branch with Id {BranchId} not found", request.BranchId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                companyResult.ErrorMessage ?? "Branch not found");
        }

        var companyServiceResult = await _branchService.CheckCompanyServiceId(new CompanyServiceRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            CompanyServiceId = request.ServiceId,
            RequestedAt = DateTimeOffset.UtcNow
        });

        if (!companyServiceResult.IsValid)
        {
            _logger.LogInformation("CompanyService with Id {CompanyServiceId} not found", request.ServiceId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                companyResult.ErrorMessage ?? "CompanyService not found");
        }



        var scheduleResponse = await _userService.CheckEmployeeAvailability(new EmployeeAvailabilityRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            StartTime = request.StartTime,
            DurationMinutes = 30
        });
        
        if (!scheduleResponse.IsAvailable)
        {
            _logger.LogWarning("Employee {EmployeeId} is not available at {StartTime}. Message: {ErrorMessage}", 
                request.EmployeeId, request.StartTime, scheduleResponse.ErrorMessage);
            
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest,scheduleResponse.ErrorMessage ?? 
                                                                         "The selected time slot is not available. Please choose a different time.");
        }

        _logger.LogInformation(
            "IDs validated successfully for Company {CompanyId}, Branch {BranchId}, Service {ServiceId}",
            request.CompanyId, request.BranchId, request.ServiceId);



        var customer = await _userService.GetCustomerById(new CustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = customerId
        });

        if (!customer.IsValid)
        {
            _logger.LogWarning("Customer with Id {id} not found for adding new queue ", customerId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Customer with Id {customerId} not found for adding new queue");
        }

        var companyId = companyResult.CompanyId;
        
        var employee = await _userService.GetEmployeeById(new EmployeeByIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = request.EmployeeId
        });

        if (!employee.IsValid)
        {
            _logger.LogWarning("Employee with Id {EmployeeId} not found", request.EmployeeId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                employee.ErrorMessage ?? $"Employee with Id {request.EmployeeId} not found");
        }

        if (employee.CompanyId != companyId)
        {
            _logger.LogWarning("Employee with Id {EmployeeId} not found for this company ", request.EmployeeId);
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest,
                $"Employee with Id {request.EmployeeId} not found for this company");
        }
        
        _logger.LogDebug("Checking for overlapping queues for EmployeeId: {employeeId}", request.EmployeeId);

        var allQueuesByEmployee = await _dbContext.Queues.Where(s => s.EmployeeId == request.EmployeeId)
            .ToListAsync(cancellationToken);

        var allQueuesByEmployeeAfterFilter =
            allQueuesByEmployee.Where(q => q.Status == QueueStatus.Pending || q.Status == QueueStatus.Confirmed);

        var newQueueStart = request.StartTime;
        var newQueueEnd = newQueueStart.AddMinutes(30);
        var isDouble = allQueuesByEmployeeAfterFilter.Any(s =>
        {
            var existingStart = s.StartTime;
            var existingEnd = s.EndTime;
            return (newQueueStart < existingEnd && newQueueEnd > existingStart) &&
                   (s.Status == QueueStatus.Confirmed || s.Status == QueueStatus.Pending);
        });


        if (isDouble)
        {
            _logger.LogWarning("Time slot is already booked for employee Id {id}", request.EmployeeId);
            throw new Exception("This slot is already booked!");
        }


        _logger.LogDebug("Checking if is customer blocked for CompanyId: {id}", request.CompanyId);

        var blocked = await _userService.IsCustomerBlockedForCompany(new IsCustomerBlockedRequest
        {
            RequestId = Guid.NewGuid(),
            CompanyId = companyId,
            CustomerId = customerId
        });

        if (blocked.IsBlocked)
        {
            _logger.LogWarning("Customer {id} is blocked from Company {companyId}", customerId,
                request.CompanyId);
            throw new Exception("You are blocked by this company!");
        }

        _logger.LogInformation("Creating new queue entity");
        var queue = new QueueEntity()
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            CustomerId = customerId,
            EmployeeId = request.EmployeeId,
            ServiceId = request.ServiceId,
            StartTime = request.StartTime,
            Status = QueueStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };


        await _dbContext.Queues.AddAsync(queue, cancellationToken);
        _logger.LogDebug("Saving new queue to repository");
        await _dbContext.SaveChangesAsync(cancellationToken);


        await _publishEndpoint.Publish(new QueueEvent
        {
            Email = userEmail,
            QueueId = queue.Id,
            CompanyId = queue.CompanyId,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            StartTime = queue.StartTime,
            EndTime = queue.EndTime,
            EventType = QueueEventType.Created,
        }, cancellationToken);


        var response = new AddQueueResponseModel()
        {
            Id = queue.Id,
            CompanyId = queue.CompanyId,
            BranchId = queue.BranchId,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            ServiceId = queue.ServiceId,
            StartTime = queue.StartTime,
            Status = queue.Status
        };

        _logger.LogInformation("Successfully added new queue with Id: {id}", queue.Id);
        return response;
    }
}