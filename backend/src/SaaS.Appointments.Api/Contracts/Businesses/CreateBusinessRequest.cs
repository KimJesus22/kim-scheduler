namespace SaaS.Appointments.Api.Contracts.Businesses;

public class CreateBusinessRequest
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }
}
