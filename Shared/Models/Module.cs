using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class Module
{
    public long Id { get; set; }

    public virtual ICollection<Functionality> Functionalities { get; set; } = new List<Functionality>();
}
