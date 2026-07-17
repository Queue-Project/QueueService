namespace QContracts.Events.Enums;

public enum UpdatedQueueStatus
{
    Pending,
    Confirmed,
    CanceledByCustomer,
    CanceledByEmployee,
    CanceledByAdmin,
    Completed
}