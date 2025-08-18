using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class RolesFunctionality
{
    public long Id { get; set; }

    public long RoleId { get; set; }

    public long FunctionalityId { get; set; }

    public virtual Functionality Functionality { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
