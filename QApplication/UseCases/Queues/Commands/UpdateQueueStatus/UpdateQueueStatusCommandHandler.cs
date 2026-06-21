using System.Net;
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
using QUserService.Contracts.Requests.BlockedCustomersRequests;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Queues.Commands.UpdateQueueStatus;

public class UpdateQueueStatusCommandHandler : IRequestHandler<UpdateQueueStatusCommand, UpdateQueueStatusResponseModel>
{
    private readonly ILogger<UpdateQueueStatusCommandHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public UpdateQueueStatusCommandHandler(ILogger<UpdateQueueStatusCommandHandler> logger,
        IQueueApplicationDbContext dbContext, IPublishEndpoint publishEndpoint, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<UpdateQueueStatusResponseModel> Handle(UpdateQueueStatusCommand request,
        CancellationToken cancellationToken)
    {
        
        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var currentEmployee = await _userService.GetCurrentEmployee(new CurrentUserRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId
        });
        var employeeId = currentEmployee.EmployeeId;
        _logger.LogInformation("Updating queue status for QueueId: {QueueId} to {NewStatus}", request.QueueId,
            request.newStatus);
        var dbQueue = await _dbContext.Queues
            .Where(s=>s.EmployeeId==employeeId)
            .FirstOrDefaultAsync(s => s.Id == request.QueueId, cancellationToken);
        if (dbQueue == null)
        {
            _logger.LogWarning("Queue with Id {QueueId} not found for this employee", request.QueueId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Queue with Id {request.QueueId} not found for this employee");
        }
        
        
        

        // _logger.LogDebug("Current queue status: {CurrentStatus}, requested new status: {NewStatus}", dbQueue.Status,
        //     request.newStatus);
        // var employeeSchedule = await _dbContext.AvailabilitySchedules.Where(s => s.EmployeeId == dbQueue.EmployeeId)
        //     .ToListAsync(cancellationToken);
        //
        // if (!employeeSchedule.Any())
        // {
        //     _logger.LogWarning("Employee with Id {id} not found in schedule entities for adding new queue",
        //         dbQueue.EmployeeId);
        //     throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(AvailabilityScheduleEntity));
        // }

        switch (dbQueue.Status)
        {
            case QueueStatus.Pending:
                if (request.newStatus != QueueStatus.Confirmed && request.newStatus != QueueStatus.CancelledByEmployee)
                {
                    _logger.LogWarning("Invalid Pending status updating for this queue Id {id}", request.QueueId);
                    throw new Exception("Pending queue can only be Confirmed or Cancelled!");
                }

                break;
            case QueueStatus.Confirmed:
                if (request.newStatus != QueueStatus.Completed && request.newStatus != QueueStatus.DidNotCome &&
                    request.newStatus != QueueStatus.CancelledByEmployee)
                {
                    _logger.LogWarning("Invalid Confirmed status updating for this queue Id {id}", request.QueueId);
                    throw new Exception("Confirmed queues can only be Completed, DidNotCome, Cancelled!");
                }

                break;
            case QueueStatus.Completed:
            case QueueStatus.CancelledByEmployee:
            case QueueStatus.CancelledByCustomer:
            case QueueStatus.CanceledByAdmin:
            case QueueStatus.DidNotCome:
                _logger.LogWarning("Invalid finalized status updating for this queue Id {id}", request.QueueId);
                throw new Exception("This queue is already finalized and cannot be updated!");
        }

        if (request.newStatus != QueueStatus.Completed && request.newStatus != QueueStatus.DidNotCome &&
            request.newStatus != QueueStatus.Confirmed)
        {
            _logger.LogWarning("Invalid status update by employee: {newStatus}", request.newStatus);
            throw new Exception("Invalid status update by employee");
        }

        
        
        var blockValidation = await _userService.IsCustomerBlockedForCompany(new IsCustomerBlockedRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = dbQueue.CustomerId,
            CompanyId = dbQueue.CompanyId
        });

        if (blockValidation.IsBlocked)
        {
            _logger.LogDebug("Customer {CustomerId} is already blocked for Company {CompanyId}", 
                dbQueue.CustomerId, dbQueue.CompanyId);
            throw new Exception("You are blocked by this company!");
        }
        
        var didNotComeCount = await _dbContext.Queues
            .Where(q => q.CustomerId == dbQueue.CustomerId && 
                        q.Status == QueueStatus.DidNotCome)
            .CountAsync(cancellationToken);
        
        if (didNotComeCount >= 3)
        {
            _logger.LogWarning("Customer {CustomerId} automatically blocked for Company {CompanyId}: 3+ DidNotCome",
                dbQueue.CustomerId, dbQueue.CompanyId);

            var blockResponse = await _userService.BlockCustomer(new BlockCustomerRequest
            {
                RequestId = Guid.NewGuid(),
                CustomerId = dbQueue.CustomerId,
                CompanyId = dbQueue.CompanyId,
                Reason = "Did not come 3 times",
                BannedUntil = DateTime.MaxValue,
                DoesBanForever = true
            }, cancellationToken);

            if (!blockResponse.Success)
            {
                _logger.LogError("Failed to block customer {CustomerId}: {ErrorMessage}", 
                    dbQueue.CustomerId, blockResponse.ErrorMessage);
                throw new Exception($"Failed to block customer: {blockResponse.ErrorMessage ?? "Unknown error"}");
            }

            _logger.LogInformation("Customer {CustomerId} blocked successfully with BlockId: {BlockId}", 
                dbQueue.CustomerId, blockResponse.BlockedCustomerId);

            throw new Exception("Customer has been automatically blocked due to multiple DidNotCome.");
        }
        
        
        
        // bool Exists(int customerId, int companyId)
        // {
        //     var customer = _dbContext.BlockedCustomers.Where(s => s.CustomerId == customerId);
        //     var company = _dbContext.BlockedCustomers.Where(s => s.CompanyId == companyId);
        //
        //     if (customer.Any() && company.Any())
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }
        //
        // if (request.newStatus == QueueStatus.DidNotCome)
        // {
        //     _logger.LogDebug("Checking DidNotCome count for CustomerId: {CustomerId}", dbQueue.CustomerId);
        //
        //     var queuesByCustomer = await _dbContext.Queues.Where(s => s.CustomerId == dbQueue.CustomerId)
        //         .ToListAsync(cancellationToken);
        //
        //     var count = queuesByCustomer.Count(s => s.Status == QueueStatus.DidNotCome);
        //     if (count >= 3 && !Exists(dbQueue.CustomerId, dbQueue.CompanyId))
        //     {
        //         _logger.LogWarning("CustomerId {id} automatically blocked for CompanyId {companyId}: 3+ DidNotCome",
        //             dbQueue.CustomerId, dbQueue.CompanyId);
        //         BlockedCustomerEntity blockedCustomer = new BlockedCustomerEntity
        //         {
        //             CustomerId = dbQueue.CustomerId,
        //             CompanyId = dbQueue.CompanyId,
        //             DoesBanForever = true,
        //             Reason = "Did not come 3 times",
        //             BannedUntil = DateTime.MaxValue,
        //             CreatedAt = DateTime.UtcNow
        //         };
        //
        //         await _dbContext.BlockedCustomers.AddAsync(blockedCustomer, cancellationToken);
        //         await _dbContext.SaveChangesAsync(cancellationToken);
        //         throw new Exception("Customer has been automatically blocked due to multiple DidNotCome.");
        //     }
        // }


        if (request.newStatus == QueueStatus.Confirmed)
        {
            DateTimeOffset startTimeUtc = dbQueue.StartTime.ToUniversalTime();
            DateTimeOffset endTimeUtc;
            if (request.EndTime.HasValue)
            {
                endTimeUtc = request.EndTime.Value.ToUniversalTime();

                _logger.LogDebug("Custom end time: {endTime} (UTC)", endTimeUtc);

                if (endTimeUtc <= startTimeUtc)
                {
                    _logger.LogError("Invalid end time. Start: {StartTime} (UTC), End: {EndTime} (UTC)", startTimeUtc,
                        endTimeUtc);
                    throw new Exception($"EndTime must be later than StartTime. " +
                                        $"Start: {startTimeUtc:dd.MM.yyyy HH:mm:ss} (UTC), " +
                                        $"End: {endTimeUtc:dd.MM.yyyy HH:mm:ss} (UTC)");
                }

                
                var availabilityResponse = await _userService.CheckEmployeeAvailability(new EmployeeAvailabilityRequest
                {
                    RequestId = Guid.NewGuid(),
                    EmployeeId = dbQueue.EmployeeId,
                    StartTime = dbQueue.StartTime,
                    EndTime = request.EndTime,  
                    ExistingQueueId = dbQueue.Id  
                });

                if (!availabilityResponse.IsAvailable)
                {
                    _logger.LogWarning("Employee {EmployeeId} is not available from {StartTime} to {EndTime}. Message: {ErrorMessage}", 
                        dbQueue.EmployeeId, dbQueue.StartTime, request.EndTime, availabilityResponse.ErrorMessage);
            
                    throw new Exception(availabilityResponse.ErrorMessage ?? 
                                        "The selected time slot is not available. Please choose a different time.");
                }
                
                
                // var slotExists = employeeSchedule.Any(s => s.AvailableSlots.Any(slot =>
                //     startTimeUtc >= slot.From && endTimeUtc <= slot.To));
                //
                // if (!slotExists)
                // {
                //     _logger.LogWarning(
                //         "Updated queue time outside employee working hours. Start: {StartTime}, End: {EndTime}",
                //         startTimeUtc, endTimeUtc);
                //     throw new Exception("The updated queue time is outside the employee's working hours.");
                // }

                var queuesByEmployee = _dbContext.Queues.Where(s => s.EmployeeId == dbQueue.EmployeeId);

                var allQueueByEmployee = await queuesByEmployee
                    .Where(q => q.Status == QueueStatus.Pending || q.Status == QueueStatus.Confirmed)
                    .Where(q => q.Id != dbQueue.Id)
                    .ToListAsync(cancellationToken);

                var isOverlap = allQueueByEmployee.Any(s =>
                    startTimeUtc < (s.EndTime.HasValue ? s.EndTime.Value : s.StartTime.AddMinutes(30)) &&
                    endTimeUtc > s.StartTime);

                if (isOverlap)
                {
                    _logger.LogWarning("Time overlap detected for EmployeeId: {EmployeeId}", dbQueue.EmployeeId);
                    throw new Exception("The updated queue time overlaps with another existing queue.");
                }

                dbQueue.EndTime = endTimeUtc;
                _logger.LogDebug("Set custom end time: {EndTime} (UTC)", endTimeUtc);
            }
            else
            {
                dbQueue.EndTime = dbQueue.StartTime.AddMinutes(30);
                _logger.LogDebug("Set default end time (30 minutes): {EndTime} (UTC)", dbQueue.EndTime);
            }
        }

        dbQueue.Status = request.newStatus;
        _logger.LogDebug("Saving status update to repository");
        await _dbContext.SaveChangesAsync(cancellationToken);


        if (dbQueue.Status == QueueStatus.Confirmed || dbQueue.Status == QueueStatus.Completed || dbQueue.Status== QueueStatus.DidNotCome)
        {
            var queueUpdatedEvent = await CreateQueueUpdatedEvent(dbQueue, request.newStatus);
            await _publishEndpoint.Publish(queueUpdatedEvent, cancellationToken);
        }


        var response = new UpdateQueueStatusResponseModel
        {
            Id = dbQueue.Id,
            CompanyId = dbQueue.CompanyId,
            BranchId = dbQueue.BranchId,
            CustomerId = dbQueue.CustomerId,
            EmployeeId = dbQueue.EmployeeId,
            ServiceId = dbQueue.ServiceId,
            StartTime = dbQueue.StartTime,
            EndTime = dbQueue.EndTime.Value,
            Status = dbQueue.Status
        };

        _logger.LogInformation("Successfully updated queue {QueueId} status to {NewStatus}", request.QueueId,
            request.newStatus);
        return response;
    }
    

    private async Task<QueueEvent> CreateQueueUpdatedEvent(QueueEntity dbQueue, QueueStatus newStatus)
    {
        
        var user = await _userService.GetUserByCustomerId(new GetUserByCustomerIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = dbQueue.CustomerId
        });

        if (!user.IsValid)
        {
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"Customer with Id {dbQueue.CustomerId} not found");
        }
        
        var userEmail = user.EmailAddress;
        
        return new QueueEvent
        {
            Email = userEmail,
            CompanyId = dbQueue.CompanyId,
            QueueId = dbQueue.Id,
            CustomerId = dbQueue.CustomerId,
            EmployeeId = dbQueue.EmployeeId,
            StartTime = dbQueue.StartTime,
            EndTime = dbQueue.EndTime,
            EventType = QueueEventType.Updated,
            CancelReason = dbQueue.CancelReason,
            Status = newStatus == QueueStatus.Confirmed
                ? UpdatedQueueStatus.Confirmed
                : UpdatedQueueStatus.Completed
        };
    }
}