namespace SaaS.Appointments.Api.Contracts.Services;

public class CreateServiceRequest
{
    public Guid BusinessId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }
}