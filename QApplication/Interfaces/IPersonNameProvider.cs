namespace QApplication.Interfaces;

public interface IPersonNameProvider
{
    Task<string> GetCustomerNameAsync(int customerId);
    Task<string> GetEmployeeNameAsync(int employeeId);
}