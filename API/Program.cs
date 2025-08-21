using API.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Connector;
using Shared.Models;
using Shared.Repositories;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ===================================================================
// SECCI�N 1: CONFIGURACI�N B�SICA DE SERVICIOS
// ===================================================================

// Servicios esenciales para una aplicaci�n MVC/API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===================================================================
// SECCI�N 2: CONFIGURACI�N DE LOGGING
// ===================================================================

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// ===================================================================
// SECCI�N 3: CONFIGURACI�N DE BASE DE DATOS
// ===================================================================

var connectionString = builder.Configuration.GetConnectionString("DB.MySQL.DefaultConnection")
                     ?? builder.Configuration["DB:MySQL:DefaultConnection"];
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se encontr� la cadena de conexi�n en la configuraci�n");
}

// Configuraci�n de Entity Framework con MySQL
builder.Services.AddDbContext<CustomDBContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql"));
    // Solo habilitar en desarrollo para evitar exposici�n de datos sensibles
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// ===================================================================
// SECCI�N 4: CONFIGURACI�N DE CACH�
// ===================================================================

builder.Services.AddMemoryCache(); // Cache en memoria para datos frecuentes
builder.Services.AddDistributedMemoryCache(); // Cache distribuido (para producci�n usar Redis o SQL Server)

// ===================================================================
// SECCI�N 5: CONFIGURACI�N DE AUTENTICACI�N JWT
// ===================================================================

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key no configurada");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Eliminar margen de tiempo para validaci�n exacta
        };

        // Configuraci�n para aceptar tokens con o sin "Bearer" (�til para WebSockets)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"];
                var accessToken = context.Request.Query["access_token"];

                // Desde header Authorization
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? token.ToString()["Bearer ".Length..].Trim()
                        : token.ToString().Trim();
                }
                // Desde query string (para WebSockets en Blazor Server)
                else if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Autenticaci�n JWT fallida: {Exception}", context.Exception);
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Acceso prohibido para usuario: {User}", context.HttpContext.User.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

// ===================================================================
// SECCI�N 6: CONFIGURACI�N DE AUTORIZACI�N
// ===================================================================

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Pol�ticas personalizadas
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Admin", "Manager"));

    // Pol�tica para usuarios activos
    options.AddPolicy("ActiveUser", policy =>
        policy.RequireClaim("IsActive", "true"));
});

// ===================================================================
// SECCI�N 7: REGISTRO DE REPOSITORIOS Y SERVICIOS DE APLICACI�N
// ===================================================================

// Repositorios de datos
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ILoginAuditRepository, LoginAuditRepository>();

// Servicios de autenticaci�n
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Servicios de utilidad
builder.Services.AddHttpContextAccessor(); // Necesario para Blazor Server
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ===================================================================
// SECCI�N 8: CONFIGURACI�N DE SWAGGER (SOLO DESARROLLO)
// ===================================================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
        Description = "API de gesti�n con autenticaci�n JWT",
        Contact = new OpenApiContact
        {
            Name = "Soporte",
            Email = "soporte@empresa.com"
        },
        License = new OpenApiLicense
        {
            Name = "Licencia MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Configurar seguridad JWT en Swagger
    options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT (con o sin 'Bearer')"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "JWT"
                }
            },
            Array.Empty<string>()
        }
    });

    // Incluir comentarios XML para documentaci�n
    try
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al cargar comentarios XML: {ex.Message}");
    }

    options.EnableAnnotations();
});

// ===================================================================
// SECCI�N 9: CONFIGURACI�N DE CORS (IMPORTANTE PARA BLAZOR SERVER)
// ===================================================================

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                  ?? new[] { "http://localhost:3000", "https://localhost:7000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Necesario para autenticaci�n con cookies/tokens
              .WithExposedHeaders("Authorization", "Refresh-Token");
    });

    options.AddPolicy("ProductionCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type", "Refresh-Token")
              .AllowCredentials()
              .WithExposedHeaders("Authorization", "Refresh-Token");
    });
});

// ===================================================================
// SECCI�N 10: RATE LIMITING (LIMITACI�N DE PETICIONES)
// ===================================================================

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100, // 100 peticiones por minuto
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

// ===================================================================
// SECCI�N 11: COMPRESI�N DE RESPUESTAS (MEJORA DE RENDIMIENTO)
// ===================================================================

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// ===================================================================
// CONSTRUCCI�N DE LA APLICACI�N
// ===================================================================

var app = builder.Build();

// ===================================================================
// SECCI�N 12: CONFIGURACI�N DEL PIPELINE DE MIDDLEWARE
// ===================================================================

// Middleware de compresi�n (debe estar al inicio)
app.UseResponseCompression();

// Configurar CORS seg�n entorno
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
    app.UseDeveloperExceptionPage();

    // Swagger siempre disponible en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
        c.ConfigObject.AdditionalItems["persistAuthorization"] = "true";
        c.DisplayRequestDuration();

        // Configurar Swagger como p�gina principal en desarrollo
        c.RoutePrefix = string.Empty; // Hace que Swagger est� en la ra�z
    });
}
else
{
    app.UseCors("ProductionCors");
    app.UseExceptionHandler("/error"); // Manejo de errores para producci�n
    app.UseHsts(); // HTTP Strict Transport Security

    // En producci�n, tambi�n habilitamos Swagger pero con ruta espec�fica
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
        c.RoutePrefix = "swagger"; // Acceso mediante /swagger
    });
}

// Redirecci�n HTTPS
app.UseHttpsRedirection();

// Rate limiting
app.UseRateLimiter();

// IMPORTANTE: El orden de estos middlewares es crucial
app.UseAuthentication(); // Primero autenticaci�n
app.UseAuthorization();  // Luego autorizaci�n

// ===================================================================
// SECCI�N 13: MIDDLEWARE PERSONALIZADO PARA MANEJO DE EXCEPCIONES
// ===================================================================

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error no manejado en la aplicaci�n");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Error interno del servidor",
            message = app.Environment.IsDevelopment() ? ex.Message : "Ocurri� un error inesperado"
        });
    }
});

// ===================================================================
// SECCI�N 14: MIDDLEWARE PARA LIMPIEZA AUTOM�TICA DE TOKENS EXPIRADOS
// ===================================================================

app.Use(async (context, next) =>
{
    // Ejecutar limpieza cada hora
    var lastCleanup = context.RequestServices.GetService<DateTime?>();
    if (lastCleanup == null || DateTime.UtcNow - lastCleanup.Value > TimeSpan.FromHours(1))
    {
        try
        {
            var refreshTokenRepo = context.RequestServices.GetRequiredService<IRefreshTokenRepository>();
            await refreshTokenRepo.CleanExpiredTokensAsync();

            var loginAuditRepo = context.RequestServices.GetRequiredService<ILoginAuditRepository>();
            await loginAuditRepo.CleanOldAuditLogsAsync(90); // Mantener logs por 90 d�as

            context.RequestServices.GetService<DateTime?>()?.Add(TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error en limpieza autom�tica de tokens");
        }
    }

    await next();
});

// ===================================================================
// SECCI�N 15: CONFIGURACI�N DE ENDPOINTS
// ===================================================================

app.MapControllers();

// Redirecci�n ra�z a Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// ===================================================================
// EJECUCI�N DE LA APLICACI�N CON PUERTO CONFIGURABLE
// ===================================================================

// Obtener el puerto de configuraci�n o usar 5001 por defecto
var port = builder.Configuration.GetValue<int>("Application:Port", 5001);
string server = builder.Configuration.GetValue<string>("Application:Server","localhost");

// Configurar la URL
app.Urls.Add($"http://{server}:{port}");
app.Urls.Add($"https://{server}:{port + 1}"); // Puerto HTTPS usualmente es +1

Console.WriteLine($"La aplicaci�n se ejecutar� en:");
Console.WriteLine($"- HTTP: http://{server}:{port}");
Console.WriteLine($"- HTTPS: https://{server}:{port + 1}");
Console.WriteLine($"- Swagger: https://{server}:{port + 1}/swagger");

app.Run();

// ===================================================================
// SECCI�N 16: DEFINICIONES DE INTERFACES Y SERVICIOS
// ===================================================================

public interface ITokenService
{
    Task<string> GenerateRefreshTokenAsync(int employeeId);
    Task<bool> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task<int> GetActiveSessionsCountAsync(int employeeId);
}

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value is string userId
        ? int.Parse(userId)
        : null;

    public string? Username => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

public class TokenService : ITokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TokenService> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> GenerateRefreshTokenAsync(int employeeId)
    {
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(token);

        var refreshToken = new RefreshToken
        {
            Token = hashedToken,
            EmployeeId = employeeId,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString()
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);
        return token;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token)
    {
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(token);
        return await _refreshTokenRepository.IsValidAsync(hashedToken);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(token);
        var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _refreshTokenRepository.RevokeAsync(hashedToken, ipAddress, "Revoked by service");
    }

    public async Task<int> GetActiveSessionsCountAsync(int employeeId)
    {
        return await _refreshTokenRepository.GetActiveTokensCountAsync(employeeId);
    }
}