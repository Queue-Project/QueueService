namespace QApplication.Caching;

public static class CacheKeys
{
    public static string CompanyById(int id)
        => $"company:{id}";

    public static string AllCompaniesKey = "companies:pages";
    

    public static string QueueId(int id) 
        => $"queue:{id}";
    
    public static string EmployeeId(int id)
        => $"queue_Employee:{id}";
    
    public static string AllQueuesHashKey => "queues:pages";
    public static string AllQueuesField(int pageNumber )
        => $"{pageNumber}";
    
    public static string CustomerQueuesHashKey(int customerId)
        => $"customer:{customerId}:queues";
    
    public static string CustomerQueuesField(int pageNumber)
        => $"{pageNumber}";
    
    public static string EmployeeQueuesHashKey(int employeeId)
        => $"employee:{employeeId}:queues";
    
    public static string EmployeeQueuesField(int pageNumber)
        => $"{pageNumber}";
    
    
    public static string EmployeeAvailabilityBase(int employeeId, DateTime date)
        => $"employee:{employeeId}:availability:{date:yyyy-MM-dd}:base";

    public static string EmployeeAvailabilityQueues(int employeeId, DateTime date)
        => $"employee:{employeeId}:availability:{date:yyyy-MM-dd}:queues";
    
}