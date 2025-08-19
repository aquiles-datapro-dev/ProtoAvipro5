using Microsoft.EntityFrameworkCore;
using Shared.Connector;
using Shared.Models;
using Shared.Repositories;
using System.Linq.Expressions;

namespace Shared.Repositories
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(CustomDBContext context) : base(context)
        {
        }

        public async Task<Role> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetChildRolesAsync(long parentRoleId)
        {
            return await _dbSet
                .Where(r => r.ParentRoleId == parentRoleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetTopLevelRolesAsync()
        {
            return await _dbSet
                .Where(r => r.ParentRoleId == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesWithChildrenAsync()
        {
            return await _dbSet
                .Include(r => r.InverseParentRole)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> SearchRolesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(r => r.Name.Contains(searchTerm) ||
                           (r.Description != null && r.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<bool> RoleNameExistsAsync(string name)
        {
            return await _dbSet
                .AnyAsync(r => r.Name == name);
        }

        public async Task<bool> RoleNameExistsAsync(string name, long excludeId)
        {
            return await _dbSet
                .AnyAsync(r => r.Name == name && r.Id != excludeId);
        }

        public async Task<int> GetEmployeeCountByRoleAsync(long roleId)
        {
            return await _context.Employees
                .Where(e => e.RoleId == roleId)
                .CountAsync();
        }

        public async Task<bool> HasChildrenAsync(long roleId)
        {
            return await _dbSet
                .AnyAsync(r => r.ParentRoleId == roleId);
        }

        public async Task<Role> GetRoleWithParentAsync(long id)
        {
            return await _dbSet
                .Include(r => r.ParentRole)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> GetRoleWithChildrenAsync(long id)
        {
            return await _dbSet
                .Include(r => r.InverseParentRole)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Role> GetRoleWithDetailsAsync(long id)
        {
            return await _dbSet
                .Include(r => r.ParentRole)
                .Include(r => r.InverseParentRole)
                .Include(r => r.Employees)
                .Include(r => r.RolesFunctionalities)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Role>> GetRolesByEmployeeAsync(long employeeId)
        {
            return await _dbSet
                .Where(r => r.Employees.Any(e => e.Id == employeeId))
                .ToListAsync();
        }

        // Métodos para árbol de roles
        public async Task<IEnumerable<Role>> GetRoleTreeAsync()
        {
            var topLevelRoles = await GetTopLevelRolesAsync();
            var rolesTree = new List<Role>();

            foreach (var role in topLevelRoles)
            {
                var roleWithChildren = await GetRoleWithChildrenRecursiveAsync(role.Id);
                rolesTree.Add(roleWithChildren);
            }

            return rolesTree;
        }

        private async Task<Role> GetRoleWithChildrenRecursiveAsync(long roleId)
        {
            var role = await GetRoleWithChildrenAsync(roleId);
            if (role != null && role.InverseParentRole.Any())
            {
                foreach (var child in role.InverseParentRole.ToList())
                {
                    var childWithChildren = await GetRoleWithChildrenRecursiveAsync(child.Id);
                    // Reemplazar el child simple con el child con hijos
                    role.InverseParentRole = role.InverseParentRole
                        .Where(c => c.Id != child.Id)
                        .Append(childWithChildren)
                        .ToList();
                }
            }
            return role;
        }

        public async Task<IEnumerable<Role>> GetDescendantsAsync(long roleId)
        {
            var descendants = new List<Role>();
            await GetDescendantsRecursiveAsync(roleId, descendants);
            return descendants;
        }

        private async Task GetDescendantsRecursiveAsync(long roleId, List<Role> descendants)
        {
            var children = await GetChildRolesAsync(roleId);
            foreach (var child in children)
            {
                descendants.Add(child);
                await GetDescendantsRecursiveAsync(child.Id, descendants);
            }
        }

        public async Task<IEnumerable<Role>> GetAncestorsAsync(long roleId)
        {
            var ancestors = new List<Role>();
            await GetAncestorsRecursiveAsync(roleId, ancestors);
            return ancestors.Reverse<Role>(); // Para tener el orden desde el root
        }

        private async Task GetAncestorsRecursiveAsync(long roleId, List<Role> ancestors)
        {
            var role = await GetRoleWithParentAsync(roleId);
            if (role?.ParentRole != null)
            {
                ancestors.Add(role.ParentRole);
                await GetAncestorsRecursiveAsync(role.ParentRole.Id, ancestors);
            }
        }

        // Override del método base para incluir relaciones
        public override async Task<Role> GetByIdAsync(long id)
        {
            return await _dbSet
                .Include(r => r.ParentRole)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        // Override para incluir relaciones en GetAll
        public override async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.ParentRole)
                .ToListAsync();
        }

        // Método para verificar si se puede eliminar un rol (sin empleados asociados)
        public async Task<bool> CanDeleteRoleAsync(long roleId)
        {
            var hasEmployees = await _context.Employees.AnyAsync(e => e.RoleId == roleId);
            var hasChildren = await HasChildrenAsync(roleId);

            return !hasEmployees && !hasChildren;
        }

        // Método para mover roles a otro padre
        public async Task<bool> ChangeParentRoleAsync(long roleId, long? newParentRoleId)
        {
            var role = await GetByIdAsync(roleId);
            if (role == null) return false;

            // Verificar que el nuevo padre no crea ciclos
            if (newParentRoleId.HasValue)
            {
                var ancestors = await GetAncestorsAsync(newParentRoleId.Value);
                if (ancestors.Any(a => a.Id == roleId))
                {
                    throw new InvalidOperationException("No se puede crear un ciclo en la jerarquía de roles");
                }
            }

            role.ParentRoleId = newParentRoleId;
            await UpdateAsync(role);

            return true;
        }
    }
}