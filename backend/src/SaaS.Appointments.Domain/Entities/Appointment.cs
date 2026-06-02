namespace SaaS.Appointments.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid ServiceId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    public string? CustomerEmail { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Business? Business { get; set; }

    public Service? Service { get; set; }
}