namespace SaaS.Appointments.Api.Auth;

// Esta clase representa la configuración necesaria para crear y validar JWT.
// Los valores reales vienen de appsettings.json en desarrollo,
// y después de variables de entorno en producción.
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
