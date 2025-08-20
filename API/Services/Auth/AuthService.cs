using Microsoft.IdentityModel.Tokens;
using Shared.Models;
using Shared.Repositories;
using Shared.Requests;
using Shared.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services.Auth
{
   
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration configuration,
            IEmployeeRepository employeeRepository,
            IRoleRepository roleRepository,
            ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _employeeRepository = employeeRepository;
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation($"Intento de login para usuario: {request.Username}");

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("El nombre de usuario es requerido");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("La contraseña es requerida");

            // Buscar el usuario
            var employee = await _employeeRepository.GetByUsernameAsync(request.Username);
            if (employee == null)
            {
                _logger.LogWarning($"Usuario no encontrado: {request.Username}");
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Verificar si está activo
            if (employee.Active != true)
            {
                _logger.LogWarning($"Usuario inactivo intentó login: {request.Username}");
                throw new UnauthorizedAccessException("Usuario inactivo");
            }

            // Verificar contraseña
            if (!VerifyPassword(request.Password, employee.Password))
            {
                _logger.LogWarning($"Contraseña incorrecta para usuario: {request.Username}");
                throw new UnauthorizedAccessException("Credenciales inválidas");
            }

            // Obtener el rol
            var role = await _roleRepository.GetByIdAsync(employee.RoleId);
            if (role == null)
            {
                _logger.LogError($"Rol no encontrado para usuario: {request.Username}, RoleId: {employee.RoleId}");
                throw new InvalidOperationException("Error en la configuración del usuario");
            }

            // Generar token
            var token = GenerateJwtToken(employee, role);

            _logger.LogInformation($"Login exitoso para usuario: {request.Username}");

            return new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(GetTokenExpirationHours()),
                UserInfo = new UserInfo
                {
                    Id = employee.Id,
                    Username = employee.Username,
                    Email = employee.Email,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Role = role.Name
                }
            };
        }

        private string GenerateJwtToken(Employee employee, Role role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim(ClaimTypes.Email, employee.Email ?? string.Empty),
                new Claim(ClaimTypes.GivenName, $"{employee.FirstName} {employee.LastName}"),
                new Claim(ClaimTypes.Role, role.Name),
                new Claim("EmployeeId", employee.Id.ToString()),
                new Claim("RoleId", role.Id.ToString()),
                new Claim("IsActive", employee.Active?.ToString() ?? "false"),
                new Claim("FullName", $"{employee.FirstName} {employee.LastName}".Trim())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(GetTokenExpirationHours()),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        

        private int GetTokenExpirationHours()
        {
            if (int.TryParse(_configuration["Jwt:ExpireHours"], out int hours))
                return hours;

            return 2; // Valor por defecto
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

       /* public async Task<bool> ValidateUserAsync(LoginRequest request)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(request.Username);
            return employee != null && VerifyPassword(request.Password, employee.Password);
        }*/

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

        public async Task<bool> ValidateCredentialsAsync(LoginRequest request)
        {
            var employee = await _employeeRepository.GetByUsernameAsync(request.Username);
            return employee != null && VerifyPassword(request.Password, employee.Password);
        }
    }
}