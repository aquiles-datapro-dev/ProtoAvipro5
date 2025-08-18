using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System;
using System.Configuration;
using System.Reflection;




var builder = WebApplication.CreateBuilder(args);

// Configuración del DbContext
/*builder.Services.AddDbContext<CustomDBContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DB.MySQLConnection"),
        new MySqlServerVersion(new Version(8, 0, 23)) // Usa tu versión de MySQL
    ));*/


// Añadir servicios de Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mi API",
        Version = "v1",
        Description = "Descripción de mi API",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tu@email.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin());

// Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
    });
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
