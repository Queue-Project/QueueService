using MagicOnion;
using QContracts.Responses;

namespace QContracts.Interfaces;

public interface IQueueService : IService<IQueueService>
{
    UnaryResult<QueueInfo> GetQueueByIdAsync(int queueId);
    
    UnaryResult<List<QueueInfo>> GetCustomerQueuesAsync(int customerId);
    UnaryResult<List<QueueInfo>> GetEmployeeQueuesAsync(int employeeId);
    UnaryResult<List<QueueInfo>> GetBranchQueuesAsync(int branchId);
    UnaryResult<List<QueueInfo>> GetCompanyQueuesAsync(int companyId);
    UnaryResult<List<QueueInfo>> GetServiceQueuesAsync(int serviceId);
    
    UnaryResult<ReviewInfo> GetQueueReviewAsync(int queueId);  
    UnaryResult<List<ReviewInfo>> GetEmployeeReviewsAsync(int employeeId);
    
    
    UnaryResult<List<ReviewInfo>> GetCustomerReviewsAsync(int customerId); 
    UnaryResult<List<ReviewInfo>> GetCompanyReviewsAsync(int companyId);  
    
    
    UnaryResult<ComplaintInfo> GetQueueComplaintAsync(int queueId);  
    UnaryResult<List<ComplaintInfo>> GetEmployeeComplaintsAsync(int employeeId);  
    UnaryResult<List<ComplaintInfo>> GetCustomerComplaintsAsync(int customerId); 
    UnaryResult<List<ComplaintInfo>> GetCompanyComplaintsAsync(int companyId);
    UnaryResult<List<CustomerInfo>> GetAllCompanyCustomers(int companyId);

}