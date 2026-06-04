using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SaaS.Appointments.Api.Auth;
using SaaS.Appointments.Infrastructure.DependencyInjection;

// WebApplication.CreateBuilder inicializa el contenedor de Inyección de Dependencias
// y carga las configuraciones desde appsettings.json y las variables de entorno.
var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddControllers();

// Lee la sección "Jwt" de appsettings.json y la convierte en JwtSettings.
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

// Registramos el servicio que genera tokens.
// Scoped significa que se crea una instancia por petición HTTP.
builder.Services.AddScoped<JwtTokenService>();

// Obtenemos la configuración JWT desde appsettings.json.
// Aquí están Issuer, Audience, SecretKey y expiración.
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwtSettings is null)
{
    throw new InvalidOperationException("La configuración JWT no existe.");
}

// Configuramos autenticación con JWT Bearer.
// Esto permite que ASP.NET Core lea tokens enviados en:
// Authorization: Bearer {token}
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Valida que el token venga del emisor esperado.
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            // Valida que el token esté dirigido a nuestra audiencia esperada.
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            // Valida que el token no esté vencido.
            ValidateLifetime = true,

            // Valida que la firma del token coincida con nuestro SecretKey.
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
            ),

            // Quitamos tolerancia extra de expiración para que el tiempo sea exacto.
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization permite usar [Authorize] en controladores/endpoints.
builder.Services.AddAuthorization();

// Configuración de CORS: Permite que el frontend (Astro corriendo en el puerto 4321)
// pueda realizar solicitudes asíncronas (fetch) a nuestra API de backend de forma segura.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:4321")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// Swagger/OpenAPI: Herramientas esenciales para el desarrollo que escanean
// nuestros controladores y generan la especificación técnica interactiva de la API.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Método de extensión personalizado: Encapsula el registro del DbContext
// y servicios de infraestructura para mantener este archivo principal limpio.
builder.Services.AddInfrastructure(builder.Configuration);

// builder.Build() compila el contenedor e inicializa el pipeline de procesamiento de peticiones.
var app = builder.Build();

// Pipeline de Middleware: El orden de estos métodos define la secuencia
// con la que una solicitud HTTP es procesada y devuelta por el servidor.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Provee la interfaz web (/swagger) para probar los endpoints interactivamente.
}

app.UseHttpsRedirection();

// UseCors debe registrarse SIEMPRE antes de MapControllers para que las cabeceras CORS
// se inyecten correctamente antes de procesar la lógica de negocio en el endpoint.
app.UseCors(FrontendCorsPolicy);

// Primero autenticamos: revisa si hay token y si es válido.
app.UseAuthentication();

// Luego autorizamos: revisa si el usuario tiene permiso.
app.UseAuthorization();

app.MapControllers(); // Asocia las peticiones HTTP entrantes a las clases controladoras basadas en sus atributos de enrutamiento.

app.Run(); // Arranca el servidor web asíncronamente y se mantiene a la escucha en el puerto configurado.
