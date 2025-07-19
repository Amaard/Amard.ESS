namespace ESS.Api.Database.Entities.Employees.Repositories;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByNationalCodeAndPhoneNumber(string nationalCode, string mobile);
}
