namespace SaaS.Appointments.Api.Contracts.Auth;

// DTO que representa los datos que el frontend manda para iniciar sesión.
// Solo necesitamos email y password; el rol se obtiene desde la base de datos.
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
