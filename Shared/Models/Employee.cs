using System;
using System.Collections.Generic;

namespace Shared.Models;

public partial class Employee
{
    public long Id { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleInitial { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? ZipCode { get; set; }

    public string? HomePhone { get; set; }

    public string? Beeper { get; set; }

    public string? Cellular { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? EmergencyRelation { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? LastWorkDate { get; set; }

    public DateTime? DateBirth { get; set; }

    public string? Title { get; set; }

    public decimal? Wages { get; set; }

    public string? Comments { get; set; }

    public string? Ss { get; set; }

    public int? Dep { get; set; }

    public string? Shop { get; set; }

    public string? MS { get; set; }

    public bool? Active { get; set; }

    public bool? Tech { get; set; }

    public bool? Insp { get; set; }

    public string? EmpIni { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? EmpMail { get; set; }

    public bool? OfficePersonal { get; set; }

    public bool? Service { get; set; }

    public string? Department { get; set; }

    public int? TotalVac { get; set; }

    public int? UsedVac { get; set; }

    public int? BalanceVac { get; set; }

    public string? SingOffTitle { get; set; }

    public bool? Ri { get; set; }

    public bool? Ip { get; set; }

    public bool? Rts { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Email { get; set; }

    public long StateId { get; set; }

    public long RoleId { get; set; }

    public virtual ICollection<EmployeesFunctionality> EmployeesFunctionalities { get; set; } = new List<EmployeesFunctionality>();

    public virtual Role Role { get; set; } = null!;

    public virtual State State { get; set; } = null!;
}
