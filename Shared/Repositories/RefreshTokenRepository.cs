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
    
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly CustomDBContext _context;
        private readonly ILogger<RefreshTokenRepository> _logger;

        public RefreshTokenRepository(CustomDBContext context, ILogger<RefreshTokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<RefreshToken> CreateAsync(RefreshToken token)
        {
            try
            {
                await _context.RefreshTokens.AddAsync(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Refresh token creado para usuario ID: {token.EmployeeId}");
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear refresh token para usuario ID: {token.EmployeeId}");
                throw;
            }
        }

        public async Task<RefreshToken?> GetByTokenAsync(string hashedToken)
        {
            try
            {
                return await _context.RefreshTokens
                    .Include(rt => rt.Employee)
                    .FirstOrDefaultAsync(rt => rt.Token == hashedToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar token por hash: {hashedToken}");
                throw;
            }
        }

        public async Task<RefreshToken?> GetByIdAsync(int id)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.Employee)
                .FirstOrDefaultAsync(rt => rt.Id == id);
        }

        public async Task RevokeAsync(string hashedToken, string revokedByIp, string reason = "Revoked")
        {
            try
            {
                var token = await GetByTokenAsync(hashedToken);
                if (token == null)
                {
                    _logger.LogWarning($"Intento de revocar token no encontrado: {hashedToken}");
                    throw new ArgumentException("Token no encontrado");
                }

                if (token.IsRevoked)
                {
                    _logger.LogWarning($"Token ya revocado: {hashedToken}");
                    return;
                }

                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = revokedByIp;
                token.ReasonRevoked = reason;

                _context.RefreshTokens.Update(token);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Token revocado exitosamente. Usuario ID: {token.EmployeeId}, Razón: {reason}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al revocar token: {hashedToken}");
                throw;
            }
        }

        public async Task RevokeAllForUserAsync(int employeeId, string revokedByIp, string reason = "System revoke")
        {
            try
            {
                var activeTokens = await _context.RefreshTokens
                    .Where(rt => rt.EmployeeId == employeeId &&
                                rt.Revoked == null &&
                                rt.Expires > DateTime.UtcNow)
                    .ToListAsync();

                if (!activeTokens.Any())
                {
                    _logger.LogInformation($"No hay tokens activos para revocar del usuario ID: {employeeId}");
                    return;
                }

                foreach (var token in activeTokens)
                {
                    token.Revoked = DateTime.UtcNow;
                    token.RevokedByIp = revokedByIp;
                    token.ReasonRevoked = reason;
                }

                _context.RefreshTokens.UpdateRange(activeTokens);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Todos los tokens revocados para usuario ID: {employeeId}. Tokens afectados: {activeTokens.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al revocar todos los tokens del usuario ID: {employeeId}");
                throw;
            }
        }

        public async Task<bool> IsValidAsync(string hashedToken)
        {
            try
            {
                var token = await GetByTokenAsync(hashedToken);
                return token != null && token.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar token: {hashedToken}");
                return false;
            }
        }

        public async Task CleanExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.RefreshTokens
                    .Where(rt => rt.Expires < DateTime.UtcNow || rt.Revoked != null)
                    .ToListAsync();

                if (!expiredTokens.Any())
                {
                    _logger.LogInformation("No hay tokens expirados para limpiar");
                    return;
                }

                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tokens expirados limpiados: {expiredTokens.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar tokens expirados");
                throw;
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(int employeeId)
        {
            try
            {
                return await _context.RefreshTokens
                    .Where(rt => rt.EmployeeId == employeeId &&
                                rt.Revoked == null &&
                                rt.Expires > DateTime.UtcNow)
                    .OrderByDescending(rt => rt.Created)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tokens activos del usuario ID: {employeeId}");
                throw;
            }
        }

        public async Task<int> GetActiveTokensCountAsync(int employeeId)
        {
            return await _context.RefreshTokens
                .CountAsync(rt => rt.EmployeeId == employeeId &&
                                 rt.Revoked == null &&
                                 rt.Expires > DateTime.UtcNow);
        }

        public async Task<bool> TokenExistsAsync(string hashedToken)
        {
            return await _context.RefreshTokens
                .AnyAsync(rt => rt.Token == hashedToken);
        }
    }
}