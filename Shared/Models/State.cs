using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class State
{
    public long Id { get; set; }

    public string? Abbr { get; set; }

    public string? Name { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}
