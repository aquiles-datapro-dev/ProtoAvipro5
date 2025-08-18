using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class Role
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long? ParentRoleId { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Role> InverseParentRole { get; set; } = new List<Role>();

    public virtual Role? ParentRole { get; set; }

    public virtual ICollection<RolesFunctionality> RolesFunctionalities { get; set; } = new List<RolesFunctionality>();
}
