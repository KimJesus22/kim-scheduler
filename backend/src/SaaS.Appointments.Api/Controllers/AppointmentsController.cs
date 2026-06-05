using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Contracts.Appointments;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AppointmentsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Crear cita será público porque un cliente debe poder reservar.
    // Más adelante este endpoint será usado por la ruta pública /book/{businessSlug}.
    [HttpPost]
    public async Task<IActionResult> CreateAppointment(CreateAppointmentRequest request)
    {
        var customerName = request.CustomerName.Trim();
        var customerPhone = request.CustomerPhone.Trim();
        var customerEmail = request.CustomerEmail?.Trim();

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return BadRequest(new
            {
                message = "El nombre del cliente es obligatorio."
            });
        }

        if (string.IsNullOrWhiteSpace(customerPhone))
        {
            return BadRequest(new
            {
                message = "El teléfono del cliente es obligatorio."
            });
        }

        // Validamos que el negocio exista y esté activo.
        var businessExists = await _dbContext.Businesses
            .AnyAsync(x => x.Id == request.BusinessId && x.IsActive);

        if (!businessExists)
        {
            return NotFound(new
            {
                message = "El negocio no existe o está inactivo."
            });
        }

        // Buscamos el servicio y verificamos que pertenezca al negocio.
        // Esto evita reservar un servicio de otro negocio por accidente.
        var service = await _dbContext.Services
            .FirstOrDefaultAsync(x =>
                x.Id == request.ServiceId &&
                x.BusinessId == request.BusinessId &&
                x.IsActive
            );

        if (service is null)
        {
            return NotFound(new
            {
                message = "El servicio no existe, está inactivo o no pertenece a este negocio."
            });
        }

        if (request.StartAtUtc <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message = "La cita debe programarse en una fecha futura."
            });
        }

        // La duración real de la cita viene del servicio.
        // Ejemplo: Corte clásico dura 45 minutos.
        var endAtUtc = request.StartAtUtc.AddMinutes(service.DurationMinutes);

        // Obtenemos día y hora a partir de StartAtUtc.
        // Deuda técnica: por ahora asumimos UTC como referencia.
        // Más adelante agregaremos zona horaria por negocio.
        var appointmentDay = request.StartAtUtc.DayOfWeek;
        var appointmentStartTime = TimeOnly.FromDateTime(request.StartAtUtc);
        var appointmentEndTime = TimeOnly.FromDateTime(endAtUtc);

        // Buscamos el horario laboral configurado para ese negocio y ese día.
        var businessHour = await _dbContext.BusinessHours
            .FirstOrDefaultAsync(x =>
                x.BusinessId == request.BusinessId &&
                x.DayOfWeek == appointmentDay
            );

        if (businessHour is null)
        {
            return BadRequest(new
            {
                message = "El negocio no tiene horario configurado para ese día."
            });
        }

        if (businessHour.IsClosed)
        {
            return BadRequest(new
            {
                message = "El negocio está cerrado ese día."
            });
        }

        // Validamos que la cita completa quepa dentro del horario laboral.
        // No basta con que inicie dentro; también debe terminar antes del cierre.
        var isInsideBusinessHours =
            appointmentStartTime >= businessHour.OpenTime &&
            appointmentEndTime <= businessHour.CloseTime;

        if (!isInsideBusinessHours)
        {
            return BadRequest(new
            {
                message = "La cita está fuera del horario laboral del negocio."
            });
        }

        // Validamos empalmes.
        // Fórmula:
        // Nueva cita empieza antes de que otra termine
        // Y nueva cita termina después de que otra empieza
        // Entonces hay choque.
        var hasOverlap = await _dbContext.Appointments
            .AnyAsync(x =>
                x.BusinessId == request.BusinessId &&
                x.Status != AppointmentStatus.Cancelled &&
                request.StartAtUtc < x.EndAtUtc &&
                endAtUtc > x.StartAtUtc
            );

        if (hasOverlap)
        {
            return Conflict(new
            {
                message = "Ya existe una cita en ese horario."
            });
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            BusinessId = request.BusinessId,
            ServiceId = request.ServiceId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = endAtUtc,
            Status = AppointmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Appointments.Add(appointment);

        // Ejecuta el INSERT real en MariaDB.
        await _dbContext.SaveChangesAsync();

        var response = new AppointmentResponse
        {
            Id = appointment.Id,
            BusinessId = appointment.BusinessId,
            ServiceId = appointment.ServiceId,
            CustomerName = appointment.CustomerName,
            CustomerPhone = appointment.CustomerPhone,
            CustomerEmail = appointment.CustomerEmail,
            StartAtUtc = appointment.StartAtUtc,
            EndAtUtc = appointment.EndAtUtc,
            Status = appointment.Status,
            CreatedAtUtc = appointment.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(GetAppointmentById),
            new { id = appointment.Id },
            response
        );
    }

    // Consultar una cita específica será útil para pruebas y futuro panel.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var appointment = await _dbContext.Appointments
            .Where(x => x.Id == id)
            .Select(x => new AppointmentResponse
            {
                Id = x.Id,
                BusinessId = x.BusinessId,
                ServiceId = x.ServiceId,
                CustomerName = x.CustomerName,
                CustomerPhone = x.CustomerPhone,
                CustomerEmail = x.CustomerEmail,
                StartAtUtc = x.StartAtUtc,
                EndAtUtc = x.EndAtUtc,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (appointment is null)
        {
            return NotFound(new
            {
                message = "Cita no encontrada."
            });
        }

        return Ok(appointment);
    }

    // Listar citas por negocio es una acción administrativa.
    // Un cliente no debería ver todas las citas del negocio.
    [Authorize(Roles = "Admin")]
    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetAppointmentsByBusiness(Guid businessId)
    {
        var appointments = await _dbContext.Appointments
            .Where(x => x.BusinessId == businessId)
            .OrderByDescending(x => x.StartAtUtc)
            .Select(x => new AppointmentResponse
            {
                Id = x.Id,
                BusinessId = x.BusinessId,
                ServiceId = x.ServiceId,
                CustomerName = x.CustomerName,
                CustomerPhone = x.CustomerPhone,
                CustomerEmail = x.CustomerEmail,
                StartAtUtc = x.StartAtUtc,
                EndAtUtc = x.EndAtUtc,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(appointments);
    }
}
