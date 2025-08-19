using Microsoft.EntityFrameworkCore;
using Shared.Connector;
using Shared.Models;
using Shared.Repositories;
using System.Linq.Expressions;

namespace Shared.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(CustomDBContext context) : base(context)
        {
        }

        public async Task<Employee> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.Username == username);
        }

        public async Task<Employee> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync()
        {
            return await _dbSet
                .Where(e => e.Active == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetInactiveEmployeesAsync()
        {
            return await _dbSet
                .Where(e => e.Active == false)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(long roleId)
        {
            return await _dbSet
                .Where(e => e.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByStateAsync(long stateId)
        {
            return await _dbSet
                .Where(e => e.StateId == stateId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(e => e.Department == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByShopAsync(string shop)
        {
            return await _dbSet
                .Where(e => e.Shop == shop)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(e => e.FirstName.Contains(searchTerm) ||
                           e.LastName.Contains(searchTerm) ||
                           e.Username.Contains(searchTerm) ||
                           e.Email.Contains(searchTerm) ||
                           e.Department.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesHiredInRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.HireDate >= startDate && e.HireDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetTechniciansAsync()
        {
            return await _dbSet
                .Where(e => e.Tech == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetInspectorsAsync()
        {
            return await _dbSet
                .Where(e => e.Insp == true)
                .ToListAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet
                .AnyAsync(e => e.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet
                .AnyAsync(e => e.Email == email);
        }

        public async Task<bool> DeactivateEmployeeAsync(long id)
        {
            var employee = await GetByIdAsync(id);
            if (employee == null) return false;

            employee.Active = false;
            employee.ModifiedAt = DateTime.UtcNow;

            _dbSet.Update(employee);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateEmployeeAsync(long id)
        {
            var employee = await GetByIdAsync(id);
            if (employee == null) return false;

            employee.Active = true;
            employee.ModifiedAt = DateTime.UtcNow;

            _dbSet.Update(employee);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task UpdatePasswordAsync(long id, string newPassword)
        {
            var employee = await GetByIdAsync(id);
            if (employee != null)
            {
                employee.Password = newPassword;
                employee.ModifiedAt = DateTime.UtcNow;

                _dbSet.Update(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateEmailAsync(long id, string newEmail)
        {
            var employee = await GetByIdAsync(id);
            if (employee != null)
            {
                employee.Email = newEmail;
                employee.ModifiedAt = DateTime.UtcNow;

                _dbSet.Update(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateVacationDaysAsync(long id, int totalVac, int usedVac, int balanceVac)
        {
            var employee = await GetByIdAsync(id);
            if (employee != null)
            {
                employee.TotalVac = totalVac;
                employee.UsedVac = usedVac;
                employee.BalanceVac = balanceVac;
                employee.ModifiedAt = DateTime.UtcNow;

                _dbSet.Update(employee);
                await _context.SaveChangesAsync();
            }
        }

        // Métodos con includes para relaciones
        public async Task<Employee> GetByIdWithDetailsAsync(long id)
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.State)
                .Include(e => e.EmployeesFunctionalities)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee> GetByUsernameWithDetailsAsync(string username)
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.State)
                .Include(e => e.EmployeesFunctionalities)
                .FirstOrDefaultAsync(e => e.Username == username);
        }

        public async Task<IEnumerable<Employee>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.State)
                .Include(e => e.EmployeesFunctionalities)
                .ToListAsync();
        }

        // Override del método base para incluir detalles
        public override async Task<Employee> GetByIdAsync(long id)
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.State)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // Override para incluir detalles en GetAll
        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.State)
                .ToListAsync();
        }
    }
}