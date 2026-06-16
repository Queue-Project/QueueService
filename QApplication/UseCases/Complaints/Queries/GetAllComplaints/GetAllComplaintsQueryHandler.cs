using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Complaints.Queries.GetAllComplaints;

public class
    GetAllComplaintsQueryHandler : IRequestHandler<GetAllComplaintsQuery, PagedResponse<ComplaintResponseModel>>
{
    private const int PageSize = 15;
    private readonly ILogger<GetAllComplaintsQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public GetAllComplaintsQueryHandler(ILogger<GetAllComplaintsQueryHandler> logger,
        IQueueApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<PagedResponse<ComplaintResponseModel>> Handle(GetAllComplaintsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all complaints. PageNumber: {pageNumber}, PageSize: {pageSize}",
            request.PageNumber,
            PageSize);

        
        
        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var isUserEmployee = await _userService.IsCurrentUserEmployee(new CurrentUserRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId
        });
        
        

        int employeeId = 0;
        int customerId = 0;
        if (isUserEmployee.IsEmployee)
        {
            var currentEmployee = await _userService.GetCurrentEmployee(new CurrentUserRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = userId
            });
            employeeId = currentEmployee.EmployeeId;
        }
        else
        {
            var currentCustomer = await _userService.GetCurrentCustomer(new CurrentUserRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = userId
            });
            customerId = currentCustomer.CustomerId;
        }


        var totalCount = await _dbContext.Complaints
            .Where(s => isUserEmployee.IsEmployee
                ? s.Queue.EmployeeId == employeeId
                : s.CustomerId == customerId)
            .CountAsync(cancellationToken);

        var dbComplaints = await _dbContext.Complaints
            .Include(s => s.Queue)
            .Where(s => isUserEmployee.IsEmployee
                ? s.Queue.EmployeeId == employeeId
                : s.CustomerId == customerId)
            .OrderBy(s => s.Id)
            .Skip((request.PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);


        var response = dbComplaints.Select(complaint => new ComplaintResponseModel()
        {
            Id = complaint.Id,
            CustomerId = complaint.CustomerId,
            QueueId = complaint.QueueId,
            EmployeeId = complaint.Queue.EmployeeId,
            ComplaintText = complaint.ComplaintText,
            ResponseText = complaint.ResponseText,
            ComplaintStatus = complaint.ComplaintStatus
        }).ToList();

        _logger.LogInformation("Fetched {complaintsCount} complaints.", response.Count);

        return new PagedResponse<ComplaintResponseModel>
        {
            Items = response,
            PageNumber = request.PageNumber,
            PageSize = PageSize,
            TotalCount = totalCount
        };
    }
}