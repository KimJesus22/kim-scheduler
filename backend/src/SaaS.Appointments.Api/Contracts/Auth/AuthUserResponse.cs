using SaaS.Appointments.Domain.Entities;

namespace SaaS.Appointments.Api.Contracts.Auth;

// DTO de respuesta para no exponer datos sensibles.
// Nunca regresamos PasswordHash al frontend.
public class AuthUserResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
