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

    [HttpPost]
    public async Task<IActionResult> CreateService(CreateServiceRequest request)
    {
        var businessExists = await _dbContext.Businesses
            .AnyAsync(x => x.Id == request.BusinessId);

        if (!businessExists)
        {
            return NotFound(new
            {
                message = "El negocio no existe."
            });
        }

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

    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetServicesByBusiness(Guid businessId)
    {
        var services = await _dbContext.Services
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
