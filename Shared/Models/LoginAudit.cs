using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class LoginAudit
    {

        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool Success { get; set; }
        public string? FailureReason { get; set; }

        public Employee? Employee { get; set; }

    }
}
