namespace QDomain.Enums;

public enum QueueStatus
{
    Pending,
    Confirmed,
    Completed, 
    CancelledByCustomer,
    CancelledByEmployee,
    CanceledByAdmin,
    DidNotCome
}