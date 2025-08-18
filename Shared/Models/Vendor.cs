using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class Vendor
{
    public long Id { get; set; }

    public string? VendorName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? ZipCode { get; set; }

    public string? Phone { get; set; }

    public string? Fax { get; set; }

    public string? Contact { get; set; }

    public string? PaymentMethod { get; set; }

    public int? Terms { get; set; }

    public decimal? BalDue { get; set; }

    public float? VendorTax { get; set; }

    public string? InternetAddress { get; set; }

    public string? AcctNumber { get; set; }

    public int? FastType { get; set; }

    public string? Email { get; set; }

    public string? ContactSecond { get; set; }

    public string? Country { get; set; }

    public string? BillAdd1 { get; set; }

    public string? BillAdd2 { get; set; }

    public string? BillCity { get; set; }

    public string? BillState { get; set; }

    public string? BillZip { get; set; }

    public string? BillCountry { get; set; }

    public bool? AuditVendor { get; set; }

    public DateTime? LastAuditDate { get; set; }

    public bool? Blacklist { get; set; }

    public DateTime? DateAuditSent { get; set; }

    public string? AuditNotes { get; set; }

    public DateTime? EasaDate { get; set; }

    public DateTime? DrugProDate { get; set; }

    public DateTime? IsoDate { get; set; }

    public DateTime? NapCapDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public long StateId { get; set; }

    public virtual State State { get; set; } = null!;

    public virtual ICollection<VendorsContact> VendorsContacts { get; set; } = new List<VendorsContact>();
}
