using Shared.Models;
using Shared.Requests;
using Shared.Responses;


namespace API.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(string username, string password);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<bool> ValidateUserAsync(string username, string password);
        string GenerateJwtToken(Employee employee);
    }
}
