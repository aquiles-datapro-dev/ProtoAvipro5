using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class Functionality
{
    public long Id { get; set; }

    public long ModuleId { get; set; }

    public virtual ICollection<EmployeesFunctionality> EmployeesFunctionalities { get; set; } = new List<EmployeesFunctionality>();

    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<RolesFunctionality> RolesFunctionalities { get; set; } = new List<RolesFunctionality>();
}
