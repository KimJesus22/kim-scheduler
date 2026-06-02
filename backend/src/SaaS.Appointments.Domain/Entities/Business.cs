namespace SaaS.Appointments.Domain.Entities;

public class Business
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Service> Services { get; set; } = new();

    public List<BusinessHour> BusinessHours { get; set; } = new();

    public List<Appointment> Appointments { get; set; } = new();
}