using SaaS.Appointments.Infrastructure.DependencyInjection;

// WebApplication.CreateBuilder inicializa el contenedor de Inyección de Dependencias
// y carga las configuraciones desde appsettings.json y las variables de entorno.
var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddControllers();

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

app.MapControllers(); // Asocia las peticiones HTTP entrantes a las clases controladoras basadas en sus atributos de enrutamiento.

app.Run(); // Arranca el servidor web asíncronamente y se mantiene a la escucha en el puerto configurado.
