using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Contracts.Services;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ServicesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // POST /api/services: Crea un nuevo servicio (ej. "Corte", "Coloración") asociado a un negocio.
    [HttpPost]
    public async Task<IActionResult> CreateService(CreateServiceRequest request)
    {
        // Integridad Referencial: Validamos que exista el negocio padre en la DB
        // antes de intentar insertar el registro del servicio secundario (FK constraint).
        var businessExists = await _dbContext.Businesses
            .AnyAsync(x => x.Id == request.BusinessId);

        if (!businessExists)
        {
            return NotFound(new
            {
                message = "El negocio no existe."
            });
        }

        // Reglas de negocio cuantitativas: Evitamos inconsistencias en variables numéricas.
        if (request.DurationMinutes <= 0)
        {
            return BadRequest(new
            {
                message = "La duración del servicio debe ser mayor a 0 minutos."
            });
        }

        if (request.Price < 0)
        {
            return BadRequest(new
            {
                message = "El precio no puede ser negativo."
            });
        }

        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = request.BusinessId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = true
        };

        _dbContext.Services.Add(service);

        await _dbContext.SaveChangesAsync();

        var response = new ServiceResponse
        {
            Id = service.Id,
            BusinessId = service.BusinessId,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            IsActive = service.IsActive
        };

        return CreatedAtAction(
            nameof(GetServiceById),
            new { id = service.Id },
            response
        );
    }

    // GET /api/services/business/{businessId:guid}: Obtiene el catálogo de servicios de un negocio específico.
    // El prefijo :guid asegura que la petición falle inmediatamente en el enrutador si el ID es un string inválido.
    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetServicesByBusiness(Guid businessId)
    {
        var services = await _dbContext.Services
            // Mapeo relacional: Filtramos por el FK del negocio e ignoramos servicios que hayan sido inactivados.
            .Where(x => x.BusinessId == businessId && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new ServiceResponse
            {
                Id = x.Id,
                BusinessId = x.BusinessId,
                Name = x.Name,
                Description = x.Description,
                DurationMinutes = x.DurationMinutes,
                Price = x.Price,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(services);
    }

    // GET /api/services/{id:guid}: Obtiene un servicio individual por su ID primario.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetServiceById(Guid id)
    {
        var service = await _dbContext.Services
            .Where(x => x.Id == id)
            .Select(x => new ServiceResponse
            {
                Id = x.Id,
                BusinessId = x.BusinessId,
                Name = x.Name,
                Description = x.Description,
                DurationMinutes = x.DurationMinutes,
                Price = x.Price,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (service is null)
        {
            return NotFound(new
            {
                message = "Servicio no encontrado."
            });
        }

        return Ok(service);
    }
}
