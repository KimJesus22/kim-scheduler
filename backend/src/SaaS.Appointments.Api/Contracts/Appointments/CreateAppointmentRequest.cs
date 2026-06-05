namespace SaaS.Appointments.Api.Contracts.Appointments;

// DTO que representa lo que un cliente manda para reservar una cita.
// El cliente elige negocio, servicio, datos de contacto y fecha/hora de inicio.
public class CreateAppointmentRequest
{
    public Guid BusinessId { get; set; }

    public Guid ServiceId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string? CustomerEmail { get; set; }

    // Por ahora trabajamos con UTC para mantener consistencia en base de datos.
    // Más adelante agregaremos zona horaria del negocio.
    public DateTime StartAtUtc { get; set; }
}
