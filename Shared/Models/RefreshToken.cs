using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public string Token { get; set; } = string.Empty; // Token hasheado
        public long EmployeeId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
        public string? ReasonRevoked { get; set; }

        // Navigation property
        public Employee? Employee { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsRevoked && !IsExpired;

    }
}
