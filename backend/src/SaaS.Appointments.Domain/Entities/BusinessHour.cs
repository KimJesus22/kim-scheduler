namespace SaaS.Appointments.Domain.Entities;

public class BusinessHour
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    public bool IsClosed { get; set; }

    public Business? Business { get; set; }
}