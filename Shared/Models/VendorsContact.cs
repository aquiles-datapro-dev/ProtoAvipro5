using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class VendorsContact
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public int? Title { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long VendorId { get; set; }

    public virtual Vendor Vendor { get; set; } = null!;
}
