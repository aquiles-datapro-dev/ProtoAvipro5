using Shared.Models;
using Shared.Repositories;
using System.Linq.Expressions;

namespace Shared.Repositories
{
    public interface IRoleRepository : IRepository<Role>
    {
        // Métodos específicos para Role
        Task<Role> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetChildRolesAsync(long parentRoleId);
        Task<IEnumerable<Role>> GetTopLevelRolesAsync();
        Task<IEnumerable<Role>> GetRolesWithChildrenAsync();
        Task<IEnumerable<Role>> SearchRolesAsync(string searchTerm);
        Task<bool> RoleNameExistsAsync(string name);
        Task<bool> RoleNameExistsAsync(string name, long excludeId);
        Task<int> GetEmployeeCountByRoleAsync(long roleId);
        Task<bool> HasChildrenAsync(long roleId);
        Task<Role> GetRoleWithParentAsync(long id);
        Task<Role> GetRoleWithChildrenAsync(long id);
        Task<Role> GetRoleWithDetailsAsync(long id);
        Task<IEnumerable<Role>> GetRolesByEmployeeAsync(long employeeId);

        // Métodos para árbol de roles
        Task<IEnumerable<Role>> GetRoleTreeAsync();
        Task<IEnumerable<Role>> GetDescendantsAsync(long roleId);
        Task<IEnumerable<Role>> GetAncestorsAsync(long roleId);
    }
}