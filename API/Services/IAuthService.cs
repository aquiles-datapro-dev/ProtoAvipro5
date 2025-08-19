using Shared.Models;
using Shared.Requests;
using Shared.Responses;


namespace API.Services
{
    public interface IAuthService
    {
         Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> ValidateCredentialsAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        string GenerateJwtToken(Employee employee);
    }
}
