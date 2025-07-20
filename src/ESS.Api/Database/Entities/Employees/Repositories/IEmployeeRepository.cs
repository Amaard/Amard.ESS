namespace ESS.Api.Database.Entities.Employees.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> ValidateEmployeeByNationalCodeAndPhoneNumber(string nationalCode, string mobile);
}
