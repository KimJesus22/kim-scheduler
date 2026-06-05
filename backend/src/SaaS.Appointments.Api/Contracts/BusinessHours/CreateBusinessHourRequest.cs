namespace SaaS.Appointments.Api.Contracts.BusinessHours;

// DTO para crear un horario laboral de un negocio.
// Ejemplo: Lunes de 09:00 a 18:00.
public class CreateBusinessHourRequest
{
    public Guid BusinessId { get; set; }

    // Usamos DayOfWeek porque .NET ya tiene representados los días:
    // Sunday = 0, Monday = 1, Tuesday = 2, etc.
    public DayOfWeek DayOfWeek { get; set; }

    // TimeOnly representa solo una hora, sin fecha.
    // Ejemplo: 09:00, 18:30.
    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    // Permite marcar un día como cerrado.
    // Ejemplo: domingo cerrado.
    public bool IsClosed { get; set; }
}
