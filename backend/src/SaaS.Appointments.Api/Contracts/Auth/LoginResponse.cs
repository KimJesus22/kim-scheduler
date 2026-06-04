using SaaS.Appointments.Domain.Entities;

namespace SaaS.Appointments.Api.Contracts.Auth;

// Respuesta que recibe el frontend cuando el login es correcto.
// Incluye datos básicos del usuario y el JWT para futuras peticiones protegidas.
public class LoginResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    // Token JWT generado por el backend.
    // El frontend lo usará después en el header:
    // Authorization: Bearer {token}
    public string Token { get; set; } = string.Empty;
}
