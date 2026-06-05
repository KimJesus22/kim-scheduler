using SaaS.Appointments.Domain.Entities;

namespace SaaS.Appointments.Api.Contracts.Appointments;

// DTO de respuesta para mostrar una cita sin exponer la entidad directamente.
public class AppointmentResponse
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid ServiceId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string? CustomerEmail { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public AppointmentStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
