using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Contracts.Businesses;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/businesses")]
public class BusinessesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public BusinessesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBusiness(CreateBusinessRequest request)
    {
        var name = request.Name.Trim();
        var slug = request.Slug.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "El nombre del negocio es obligatorio."
            });
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return BadRequest(new
            {
                message = "El slug del negocio es obligatorio."
            });
        }

        if (slug.Contains(' '))
        {
            return BadRequest(new
            {
                message = "El slug no debe contener espacios. Usa guiones, por ejemplo: barberia-nova."
            });
        }

        var slugAlreadyExists = await _dbContext.Businesses
            .AnyAsync(x => x.Slug == slug);

        if (slugAlreadyExists)
        {
            return Conflict(new
            {
                message = "Ya existe un negocio con ese slug."
            });
        }

        var business = new Business
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Businesses.Add(business);

        await _dbContext.SaveChangesAsync();

        var response = new BusinessResponse
        {
            Id = business.Id,
            Name = business.Name,
            Slug = business.Slug,
            Phone = business.Phone,
            Email = business.Email,
            IsActive = business.IsActive,
            CreatedAtUtc = business.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(GetBusinessById),
            new { id = business.Id },
            response
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetBusinesses()
    {
        var businesses = await _dbContext.Businesses
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new BusinessResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Phone = x.Phone,
                Email = x.Email,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(businesses);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBusinessById(Guid id)
    {
        var business = await _dbContext.Businesses
            .Where(x => x.Id == id)
            .Select(x => new BusinessResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Phone = x.Phone,
                Email = x.Email,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (business is null)
        {
            return NotFound(new
            {
                message = "Negocio no encontrado."
            });
        }

        return Ok(business);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateBusiness(Guid id)
    {
        var business = await _dbContext.Businesses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (business is null)
        {
            return NotFound(new
            {
                message = "Negocio no encontrado."
            });
        }

        if (!business.IsActive)
        {
            return BadRequest(new
            {
                message = "El negocio ya está desactivado."
            });
        }

        business.IsActive = false;

        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Negocio desactivado correctamente.",
            business.Id,
            business.Name,
            business.Slug,
            business.IsActive
        });
    }
}
