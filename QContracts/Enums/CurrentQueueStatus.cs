namespace QContracts.Enums;

public enum CurrentQueueStatus
{
    Pending,
    Confirmed,
    Completed, 
    CancelledByCustomer,
    CancelledByEmployee,
    CanceledByAdmin,
    DidNotCome
}