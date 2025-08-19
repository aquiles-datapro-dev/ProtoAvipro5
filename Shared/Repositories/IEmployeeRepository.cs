using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        // Métodos específicos para Employee
        Task<Employee> GetByUsernameAsync(string username);
        Task<Employee> GetByEmailAsync(string email);
        Task<IEnumerable<Employee>> GetActiveEmployeesAsync();
        Task<IEnumerable<Employee>> GetInactiveEmployeesAsync();
        Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(long roleId);
        Task<IEnumerable<Employee>> GetEmployeesByStateAsync(long stateId);
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department);
        Task<IEnumerable<Employee>> GetEmployeesByShopAsync(string shop);
        Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm);
        Task<IEnumerable<Employee>> GetEmployeesHiredInRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Employee>> GetTechniciansAsync();
        Task<IEnumerable<Employee>> GetInspectorsAsync();
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> DeactivateEmployeeAsync(long id);
        Task<bool> ActivateEmployeeAsync(long id);
        Task UpdatePasswordAsync(long id, string newPassword);
        Task UpdateEmailAsync(long id, string newEmail);
        Task UpdateVacationDaysAsync(long id, int totalVac, int usedVac, int balanceVac);

        // Métodos con includes para relaciones
        Task<Employee> GetByIdWithDetailsAsync(long id);
        Task<Employee> GetByUsernameWithDetailsAsync(string username);
        Task<IEnumerable<Employee>> GetAllWithDetailsAsync();
    }
}
