
using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using Shared.Repositories;
using Shared.Requests;
using Shared.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
   
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRoleRepository _roleRepository;

        public AuthService(IConfiguration configuration, IEmployeeRepository employeeRepository, IRoleRepository roleRepository)
        {
            _configuration = configuration;
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
        }

        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(username);

            if (employee == null || !VerifyPassword(password, employee.Password))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            if (!employee.Active.GetValueOrDefault())
            {
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            var token = GenerateJwtToken(employee);
            var role = await _roleRepository.GetByIdAsync(employee.RoleId);

            return new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(2),
                UserInfo = new UserInfo
                {
                    Id = employee.Id,
                    Username = employee.Username,
                    Email = employee.Email,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = role?.Name ?? "Usuario"
                }
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Verificar si el usuario ya existe
            var existingUser = await _employeeRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                throw new ArgumentException("El nombre de usuario ya existe");
            }

            // Buscar rol por defecto (puedes ajustar esto)
            var defaultRole = (await _roleRepository.FindAsync(r => r.Name == "User")).FirstOrDefault();
            if (defaultRole == null)
            {
                throw new InvalidOperationException("No se encontró un rol por defecto");
            }

            var employee = new Employee
            {
                Username = request.Username,
                Password = HashPassword(request.Password),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Active = true,
                RoleId = defaultRole.Id,
                StateId = 1, // Estado por defecto, ajusta según tu DB
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            await _employeeRepository.AddAsync(employee);

            var token = GenerateJwtToken(employee);

            return new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(2),
                UserInfo = new UserInfo
                {
                    Id = employee.Id,
                    Username = employee.Username,
                    Email = employee.Email,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = defaultRole.Name
                }
            };
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(username);
            return employee != null && VerifyPassword(password, employee.Password);
        }

        public string GenerateJwtToken(Employee employee)
        {
            var role = _roleRepository.GetByIdAsync(employee.RoleId).Result;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim(ClaimTypes.Email, employee.Email ?? ""),
                new Claim(ClaimTypes.GivenName, $"{employee.FirstName} {employee.LastName}"),
                new Claim(ClaimTypes.Role, role?.Name ?? "User"),
                new Claim("EmployeeId", employee.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedPassword);
        }
    }
}