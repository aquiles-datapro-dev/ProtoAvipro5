using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Connector;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shared.Repositories
{
   
    public class LoginAuditRepository : ILoginAuditRepository
    {
        private readonly CustomDBContext _context;
        private readonly ILogger<LoginAuditRepository> _logger;

        public LoginAuditRepository(CustomDBContext context, ILogger<LoginAuditRepository> logger)
        {
            _context = context;
            _logger = logger;
        }



        public async Task LogLoginAttemptAsync(int employeeId, string ipAddress, string userAgent, bool success, string? failureReason = null)
        {
            try
            {
                var auditLog = new LoginAudit
                {
                    EmployeeId = employeeId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent.Length > 500 ? userAgent[..500] : userAgent,
                    Success = success,
                    FailureReason = failureReason?.Length > 200 ? failureReason[..200] : failureReason,
                    LoginTime = DateTime.UtcNow
                };

                await _context.LoginAudits.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                var logLevel = success ? LogLevel.Information : LogLevel.Warning;
                _logger.Log(logLevel,
                    $"Intento de login {(success ? "exitoso" : "fallido")} - " +
                    $"Usuario ID: {employeeId}, IP: {ipAddress}, Razón: {failureReason ?? "N/A"}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"Error al registrar intento de login - " +
                    $"Usuario ID: {employeeId}, IP: {ipAddress}");
                // No throw para no afectar el flujo principal de login
            }
        }

        public async Task<IEnumerable<LoginAudit>> GetUserLoginHistoryAsync(int employeeId, int days = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-days);

                return await _context.LoginAudits
                    .Where(la => la.EmployeeId == employeeId && la.LoginTime >= cutoffDate)
                    .OrderByDescending(la => la.LoginTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener historial de login del usuario ID: {employeeId}");
                throw;
            }
        }

        public async Task<IEnumerable<LoginAudit>> GetFailedLoginAttemptsAsync(string ipAddress, DateTime since)
        {
            try
            {
                return await _context.LoginAudits
                    .Where(la => la.IpAddress == ipAddress &&
                                !la.Success &&
                                la.LoginTime >= since)
                    .OrderByDescending(la => la.LoginTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener intentos fallidos para IP: {ipAddress}");
                throw;
            }
        }

        public async Task<int> GetFailedAttemptsCountAsync(int employeeId, DateTime since)
        {
            return await _context.LoginAudits
                .CountAsync(la => la.EmployeeId == employeeId &&
                                 !la.Success &&
                                 la.LoginTime >= since);
        }

        public async Task<int> GetFailedAttemptsCountByIpAsync(string ipAddress, DateTime since)
        {
            return await _context.LoginAudits
                .CountAsync(la => la.IpAddress == ipAddress &&
                                 !la.Success &&
                                 la.LoginTime >= since);
        }

        public async Task CleanOldAuditLogsAsync(int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

                var oldLogs = await _context.LoginAudits
                    .Where(la => la.LoginTime < cutoffDate)
                    .ToListAsync();

                if (!oldLogs.Any())
                {
                    _logger.LogInformation("No hay logs antiguos para limpiar");
                    return;
                }

                _context.LoginAudits.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Logs de auditoría limpiados: {oldLogs.Count}. Manteniendo logs desde: {cutoffDate:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar logs antiguos de auditoría");
                throw;
            }
        }

        public async Task<LoginAudit?> GetLastSuccessfulLoginAsync(int employeeId)
        {
            return await _context.LoginAudits
                .Where(la => la.EmployeeId == employeeId && la.Success)
                .OrderByDescending(la => la.LoginTime)
                .FirstOrDefaultAsync();
        }

        public async Task<LoginAudit?> GetLastFailedLoginAsync(int employeeId)
        {
            return await _context.LoginAudits
                .Where(la => la.EmployeeId == employeeId && !la.Success)
                .OrderByDescending(la => la.LoginTime)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LoginAudit>> GetRecentSuspiciousActivityAsync(DateTime since)
        {
            try
            {
                // Buscar actividad sospechosa: múltiples intentos fallidos desde la misma IP
                var suspiciousIPs = await _context.LoginAudits
                    .Where(la => !la.Success && la.LoginTime >= since)
                    .GroupBy(la => la.IpAddress)
                    .Where(g => g.Count() > 5) // Más de 5 intentos fallidos
                    .Select(g => g.Key)
                    .ToListAsync();

                if (!suspiciousIPs.Any())
                    return Enumerable.Empty<LoginAudit>();

                return await _context.LoginAudits
                    .Where(la => suspiciousIPs.Contains(la.IpAddress) && la.LoginTime >= since)
                    .OrderByDescending(la => la.LoginTime)
                    .ThenBy(la => la.IpAddress)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener actividad sospechosa");
                throw;
            }
        }
    }
}