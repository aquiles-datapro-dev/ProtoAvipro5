using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Repositories
{
    public interface ILoginAuditRepository
    {

        Task LogLoginAttemptAsync(int employeeId, string ipAddress, string userAgent, bool success, string? failureReason = null);
        Task<IEnumerable<LoginAudit>> GetUserLoginHistoryAsync(int employeeId, int days = 30);
        Task CleanOldAuditLogsAsync(int daysToKeep = 90);
    }
}
