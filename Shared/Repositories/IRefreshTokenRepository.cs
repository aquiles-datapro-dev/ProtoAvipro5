using Shared.Models;

namespace Shared.Repositories
{
    public interface IRefreshTokenRepository
    {

        Task<RefreshToken> CreateAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string hashedToken);
        Task RevokeAsync(string hashedToken, string revokedByIp, string reason = "Revoked");
        Task RevokeAllForUserAsync(int employeeId, string revokedByIp, string reason = "System revoke");
        Task<bool> IsValidAsync(string hashedToken);
        Task CleanExpiredTokensAsync();
        Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(int employeeId);
        Task<int> GetActiveTokensCountAsync(int employeeId);
    }
}
