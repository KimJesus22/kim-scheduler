namespace SaaS.Appointments.Domain.Entities;

// Define los roles básicos del sistema.
// No dejamos que el frontend elija libremente estos valores al registrarse.
// El backend debe asignarlos para evitar que cualquiera se vuelva Admin.
public enum UserRole
{
    Admin = 1,
    Staff = 2
}
