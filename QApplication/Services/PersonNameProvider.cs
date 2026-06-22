using QApplication.Interfaces;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.EmployeeRequests;

namespace QApplication.Services;

public class PersonNameProvider : IPersonNameProvider
{
    private readonly IUserService _userService;

    public PersonNameProvider(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<string> GetCustomerNameAsync(int customerId)
    {
        var customer = await _userService.GetCustomerById(new CustomerByIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = customerId
        });

        return customer.IsValid ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer";
    }

    public async Task<string> GetEmployeeNameAsync(int employeeId)
    {
        var employee = await _userService.GetEmployeeById(new EmployeeByIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = employeeId
        });

        return employee.IsValid ? $"{employee.FirstName} {employee.LastName}" : "Unknown Employee";
    }
}