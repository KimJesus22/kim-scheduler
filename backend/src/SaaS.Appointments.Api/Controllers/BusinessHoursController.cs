using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Contracts.BusinessHours;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/business-hours")]
public class BusinessHoursController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public BusinessHoursController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Crear horarios es una acción administrativa.
    // Un cliente público puede ver horarios, pero no debe poder modificarlos.
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateBusinessHour(CreateBusinessHourRequest request)
    {
        // Validamos que el negocio exista antes de crearle horarios.
        // Así evitamos horarios huérfanos o errores feos de llave foránea.
        var businessExists = await _dbContext.Businesses
            .AnyAsync(x => x.Id == request.BusinessId && x.IsActive);

        if (!businessExists)
        {
            return NotFound(new
            {
                message = "El negocio no existe o está inactivo."
            });
        }

        // Si el día NO está cerrado, la hora de apertura debe ser menor que la de cierre.
        // Ejemplo válido: 09:00 - 18:00.
        // Ejemplo inválido: 18:00 - 09:00.
        if (!request.IsClosed && request.OpenTime >= request.CloseTime)
        {
            return BadRequest(new
            {
                message = "La hora de apertura debe ser menor que la hora de cierre."
            });
        }

        // Evitamos duplicar horarios para el mismo negocio y día.
        // Ejemplo: no queremos dos horarios diferentes para el mismo lunes del mismo negocio.
        var dayAlreadyConfigured = await _dbContext.BusinessHours
            .AnyAsync(x =>
                x.BusinessId == request.BusinessId &&
                x.DayOfWeek == request.DayOfWeek
            );

        if (dayAlreadyConfigured)
        {
            return Conflict(new
            {
                message = "Ese día ya tiene horario configurado para este negocio."
            });
        }

        // Si el día está cerrado, normalizamos las horas a 00:00.
        // Así evitamos datos raros como:
        // IsClosed = true, OpenTime = 15:00, CloseTime = 10:00.
        //
        // Si el día está abierto, usamos las horas enviadas por el frontend.
        var openTime = request.IsClosed
            ? TimeOnly.MinValue
            : request.OpenTime;

        var closeTime = request.IsClosed
            ? TimeOnly.MinValue
            : request.CloseTime;

        var businessHour = new BusinessHour
        {
            Id = Guid.NewGuid(),
            BusinessId = request.BusinessId,
            DayOfWeek = request.DayOfWeek,
            OpenTime = openTime,
            CloseTime = closeTime,
            IsClosed = request.IsClosed
        };

        _dbContext.BusinessHours.Add(businessHour);

        // Ejecuta el INSERT real en MariaDB.
        await _dbContext.SaveChangesAsync();

        var response = new BusinessHourResponse
        {
            Id = businessHour.Id,
            BusinessId = businessHour.BusinessId,
            DayOfWeek = businessHour.DayOfWeek,
            OpenTime = businessHour.OpenTime,
            CloseTime = businessHour.CloseTime,
            IsClosed = businessHour.IsClosed
        };

        return CreatedAtAction(
            nameof(GetBusinessHoursByBusiness),
            new { businessId = businessHour.BusinessId },
            response
        );
    }

    // Este GET lo dejamos público porque más adelante la página pública de reserva
    // necesitará saber cuándo abre el negocio.
    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetBusinessHoursByBusiness(Guid businessId)
    {
        var businessExists = await _dbContext.Businesses
            .AnyAsync(x => x.Id == businessId && x.IsActive);

        if (!businessExists)
        {
            return NotFound(new
            {
                message = "El negocio no existe o está inactivo."
            });
        }

        var businessHours = await _dbContext.BusinessHours
            .Where(x => x.BusinessId == businessId)
            .OrderBy(x => x.DayOfWeek)
            .Select(x => new BusinessHourResponse
            {
                Id = x.Id,
                BusinessId = x.BusinessId,
                DayOfWeek = x.DayOfWeek,
                OpenTime = x.OpenTime,
                CloseTime = x.CloseTime,
                IsClosed = x.IsClosed
            })
            .ToListAsync();

        return Ok(businessHours);
    }
}
