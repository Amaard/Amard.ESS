using ESS.Api.Database.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ESS.Api.Database.Entities.Employees.Repositories;

public sealed class EmployeeRepository(IafDbContext dbContext) : IEmployeeRepository
{
    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        List<Employee> employeeList = await dbContext.EmployeeInfoView
            .AsNoTracking()
            .ToListAsync();

        if (employeeList is null)
        {
            return [];
        }

        return employeeList;
    }

    public async Task<Employee?> GetEmployeeByNationalCodeAndPhoneNumber(string nationalCode , string mobile)
    {
        return await dbContext.EmployeeInfoView
            .FirstOrDefaultAsync(e => e.MelliCode == nationalCode && e.Mobile == mobile);
    }
}
