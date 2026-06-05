namespace SaaS.Appointments.Api.Contracts.BusinessHours;

// DTO que devuelve la API al consultar horarios.
// No exponemos la entidad directamente para mantener control sobre la respuesta.
public class BusinessHourResponse
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    public bool IsClosed { get; set; }
}
