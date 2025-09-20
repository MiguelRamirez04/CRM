using CRM.Config;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog temprano para capturar logs de startup
builder.Host.UseSerilog();

// Agregar configuraciones centralizadas
builder.Services.AddLoggingConfiguration(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddValidationConfiguration();
builder.Services.AddMediatRConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Servicios básicos de ASP.NET Core
builder.Services.AddControllers();

// CORS si es necesario
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // Angular
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API V1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.UseHealthChecksConfiguration();

// Controladores
app.MapControllers();

// Logging de inicio
app.Logger.LogInformation("🚀 CRM API iniciada correctamente");

app.Run();
