using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Connector;
using Shared.Repositories;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 1. CONFIGURAR DbContext CORRECTAMENTE
var connectionString = builder.Configuration["DB:MySQL:DefaultConnection"];
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se encontró la cadena de conexión en la configuración");
}

builder.Services.AddDbContext<CustomDBContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql")));

// 2. CONFIGURAR JWT AUTHENTICATION SIN "Bearer" REQUERIDO
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key no configurada en appsettings.json");
}

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // ?? CONFIGURACIÓN PARA ACEPTAR TOKENS SIN "Bearer"
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(token))
                {
                    // Quitar "Bearer " si está presente, sino usar el token directamente
                    if (token.ToString().StartsWith("Bearer "))
                    {
                        context.Token = token.ToString().Substring("Bearer ".Length).Trim();
                    }
                    else
                    {
                        context.Token = token;
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

// 3. CONFIGURAR AUTORIZACIÓN
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Admin", "Manager"));
});

// 4. REGISTRAR REPOSITORIOS Y SERVICIOS
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 5. CONFIGURAR SWAGGER
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
        Description = "Descripción de mi API"
    });

    // Configurar seguridad JWT en Swagger - SIN "Bearer"
    options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey, // Cambiado a ApiKey
        In = ParameterLocation.Header,
        Description = "Ingrese solo el token JWT (sin 'Bearer')\n\nEjemplo: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
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
            new string[] {}
        }
    });

    // XML comments (opcional)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// 6. CONFIGURAR CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 7. CONFIGURAR MIDDLEWARE
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
        c.ConfigObject.AdditionalItems["persistAuthorization"] = "true";
    });
}

app.UseHttpsRedirection();

// 8. MIDDLEWARE DE AUTENTICACIÓN - IMPORTANTE: Este debe ir en el orden correcto
app.UseAuthentication();
app.UseAuthorization();

// 9. MIDDLEWARE PERSONALIZADO PARA ENDPOINTS PÚBLICOS (OPCIONAL)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    var publicPaths = new[] { "/api/auth/login", "/api/auth/register", "/api/auth/health", "/swagger", "/favicon.ico" };

    if (publicPaths.Any(p => path.StartsWith(p)))
    {
        await next();
    }
    else
    {
        // Para endpoints protegidos, el authentication middleware ya se aplica
        await next();
    }
});

app.MapControllers();

app.Run();