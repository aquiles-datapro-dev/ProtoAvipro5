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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 1. CONFIGURACIÓN DE LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// 2. CONFIGURAR DbContext CORRECTAMENTE
var connectionString = builder.Configuration.GetConnectionString("DB.MySQL.DefaultConnection")
                     ?? builder.Configuration["DB:MySQL:DefaultConnection"];
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se encontró la cadena de conexión en la configuración");
}

builder.Services.AddDbContext<CustomDBContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql"));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// 3. CONFIGURACIÓN DE CACHE
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache(); // Para production usar Redis o SQL Server


// 5. CONFIGURAR JWT AUTHENTICATION
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
            ClockSkew = TimeSpan.Zero // Eliminar skew para validación exacta
        };

        // Configuración para aceptar tokens con o sin "Bearer"
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
                // Desde query string (para WebSockets)
                else if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Autenticación JWT fallida: {Exception}", context.Exception);
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

// 6. CONFIGURAR AUTORIZACIÓN
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Admin", "Manager"));

    // Nueva política para usuarios activos
    options.AddPolicy("ActiveUser", policy =>
        policy.RequireClaim("IsActive", "true"));
});

// 7. REGISTRAR REPOSITORIOS Y SERVICIOS
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ILoginAuditRepository, LoginAuditRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>(); // Nuevo servicio para gestión de tokens

// Servicios de aplicación
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 8. CONFIGURAR SWAGGER
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
        Description = "API de gestión con autenticación JWT",
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
        Description = "Ingrese el token JWT (con o sin 'Bearer')\n\nEjemplo: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\nO simplemente: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
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

    // Incluir comentarios XML
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

    // Habilitar anotaciones
    options.EnableAnnotations();
});

// 9. CONFIGURAR CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                  ?? new[] { "http://localhost:3000", "https://localhost:7000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
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

// 10. RATE LIMITING (Básico)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

// 11. COMPRESIÓN DE RESPUESTAS
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// 12. CONFIGURAR MIDDLEWARE
app.UseResponseCompression();

// Configurar CORS según entorno
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCors");
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseCors("ProductionCors");
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Swagger configuration
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
    c.ConfigObject.AdditionalItems["persistAuthorization"] = "true";
    c.DisplayRequestDuration();

    if (!app.Environment.IsDevelopment())
    {
        c.DocumentTitle = "Mi API - Production";
    }
});

app.UseHttpsRedirection();

// Rate limiting
app.UseRateLimiter();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 13. MIDDLEWARE PERSONALIZADO PARA MANEJO DE EXCEPCIONES
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error no manejado en la aplicación");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Error interno del servidor",
            message = app.Environment.IsDevelopment() ? ex.Message : "Ocurrió un error inesperado"
        });
    }
});

// 14. MIDDLEWARE PARA LIMPIEZA AUTOMÁTICA DE TOKENS EXPIRADOS
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
            await loginAuditRepo.CleanOldAuditLogsAsync(90); // Mantener logs por 90 días

            context.RequestServices.GetService<DateTime?>()?.Add(TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error en limpieza automática de tokens");
        }
    }

    await next();
});

app.MapControllers();

// 15. ENDPOINTS PERSONALIZADOS
/*app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/api/version", () => new
{
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
});

app.MapGet("/error", () => Results.Problem("Error interno del servidor"));*/

app.Run();

// 16. INTERFACES ADICIONALES PARA NUEVOS SERVICIOS
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