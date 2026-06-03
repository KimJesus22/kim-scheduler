using SaaS.Appointments.Domain.Entities;

namespace SaaS.Appointments.Api.Contracts.Auth;

// Respuesta temporal del login.
// Más adelante aquí agregaremos el JWT.
public class LoginResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    // TODO: En el siguiente bloque agregaremos:
    // public string Token { get; set; } = string.Empty;
}
