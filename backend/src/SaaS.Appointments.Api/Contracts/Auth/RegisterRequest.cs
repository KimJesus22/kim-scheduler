namespace SaaS.Appointments.Api.Contracts.Auth;

// DTO que representa los datos que el frontend manda al registrarse.
// No incluimos Role porque el usuario no debe decidir si será Admin o Staff.
public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
