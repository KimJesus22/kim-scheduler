namespace SaaS.Appointments.Domain.Entities;

public class Service
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public Business? Business { get; set; }

    public List<Appointment> Appointments { get; set; } = new();
}