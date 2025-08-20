using API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Requests;
using Shared.Responses;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Usuario y contraseña son requeridos");
                }

                var authResponse = await _authService.LoginAsync(request);

                _logger.LogInformation($"Usuario {request.Username} ha iniciado sesión exitosamente");

                return Ok(authResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Intento de login fallido para usuario: {request.Username}");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error durante el login para usuario: {request.Username}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Usuario y contraseña son requeridos");
                }

                if (request.Password.Length < 6)
                {
                    return BadRequest("La contraseña debe tener al menos 6 caracteres");
                }

                var authResponse = await _authService.RegisterAsync(request);

                _logger.LogInformation($"Nuevo usuario registrado: {request.Username}");

                return Ok(authResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error durante el registro para usuario: {request.Username}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // En JWT, el logout es del lado del cliente (eliminar el token)
            _logger.LogInformation($"Usuario {User.Identity.Name} ha cerrado sesión");

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }

        [HttpGet("validate")]
        [Authorize]
        public IActionResult Validate()
        {
            // Endpoint para validar que el token sigue siendo válido
            var userId = User.FindFirst("EmployeeId")?.Value;
            var username = User.Identity.Name;

            return Ok(new
            {
                message = "Token válido",
                userId,
                username
            });
        }
    }
}