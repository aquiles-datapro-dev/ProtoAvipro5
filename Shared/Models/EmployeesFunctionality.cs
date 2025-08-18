using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class EmployeesFunctionality
{
    public long Id { get; set; }

    public long EmployeeId { get; set; }

    public long FunctionalityId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Functionality Functionality { get; set; } = null!;
}
