namespace SaaS.Appointments.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    // En el futuro puede conectar al usuario con un negocio específico.
    // Lo dejamos nullable porque quizá el primer usuario o staff aún no tenga negocio asignado.
    public Guid? BusinessId { get; set; }

    public string FullName { get; set; } = string.Empty;

    // El email será usado para login, por eso debe ser único.
    public string Email { get; set; } = string.Empty;

    // Nunca guardamos la contraseña en texto plano.
    // Aquí se guarda el hash generado por PasswordHasher.
    public string PasswordHash { get; set; } = string.Empty;

    // Rol asignado por el backend, no por el usuario desde el frontend.
    public UserRole Role { get; set; } = UserRole.Staff;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Relación opcional con Business.
    public Business? Business { get; set; }
}
