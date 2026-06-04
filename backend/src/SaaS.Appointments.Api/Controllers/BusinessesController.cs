using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Contracts.Businesses;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

// CONECTOR DE INFRAESTRUCTURA (AppDbContext y MariaDB):
// El controlador se comunica con la base de datos MariaDB a través de 'AppDbContext'.
// Entity Framework Core actúa como un mapeador relacional de objetos (ORM), traduciendo
// los métodos asíncronos de C# en consultas SQL optimizadas ejecutadas en el motor de base de datos.
[ApiController]
[Route("api/businesses")]
public class BusinessesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    // INYECCIÓN DE DEPENDENCIAS: El framework ASP.NET Core gestiona el ciclo de vida (Scoped)
    // del AppDbContext y lo inyecta directamente al instanciar este controlador por petición.
    public BusinessesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // POST /api/businesses (Crear Negocio):
    // Endpoint RESTful encargado del registro y persistencia de un nuevo negocio local.
    // Solo usuarios autenticados podrán crear negocios.
    // Más adelante podemos limitarlo a Admin con:
    // [Authorize(Roles = "Admin")]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateBusiness(CreateBusinessRequest request)
    {
        // USO DE DTOs (Data Transfer Objects):
        // 'CreateBusinessRequest' y 'BusinessResponse' son DTOs.
        // Los usamos para desacoplar el contrato externo de la API de nuestras entidades del dominio ('Business').
        // Esto protege la base de datos de ataques de sobre-asignación (Mass Assignment) y evita exponer campos sensibles.

        var name = request.Name.Trim();
        var slug = request.Slug.Trim().ToLower();

        // VALIDACIÓN ANTES DE GUARDAR (Fallo Rápido / Fail-Fast):
        // Validamos a nivel de servidor para asegurar consistencia del negocio y prevenir
        // que lleguen datos inválidos o corruptos al motor MariaDB, lo cual provocaría excepciones costosas de base de datos.
        if (string.IsNullOrWhiteSpace(name))
        {
            // BadRequest (HTTP 400): Informa al cliente que la petición contiene errores de validación.
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

        // AnyAsync: Traduce a un SELECT EXISTS(1) en MariaDB de forma asíncrona.
        // Es la forma más rápida y óptima de comprobar existencia en la base de datos sin traer datos innecesarios a memoria.
        var slugAlreadyExists = await _dbContext.Businesses
            .AnyAsync(x => x.Slug == slug);

        if (slugAlreadyExists)
        {
            // Conflict (HTTP 409): Indica un choque de estado. Aquí previene violar el índice único de la tabla 'businesses' en MariaDB.
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

        // TODO: En una arquitectura limpia formal, el controlador nunca debería interactuar
        // directamente con EF Core ni inicializar entidades. Debería delegar esta tarea a la capa
        // de aplicación (Application Layer) a través de comandos y controladores de casos de uso (MediatR/Services).
        _dbContext.Businesses.Add(business);

        // SaveChangesAsync: Confirma y escribe la transacción en MariaDB.
        // EF Core genera un script SQL 'INSERT INTO businesses...' de forma asíncrona y no bloqueante.
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

        // CreatedAtAction (HTTP 201 Created): Retorna el estado exitoso de creación,
        // el cuerpo del objeto creado, e inyecta la cabecera 'Location' con la URI para consultar el nuevo recurso.
        return CreatedAtAction(
            nameof(GetBusinessById),
            new { id = business.Id },
            response
        );
    }

    // GET /api/businesses (Listar Negocios Activos):
    // Endpoint que devuelve la colección de comercios locales operativos.
    [HttpGet]
    public async Task<IActionResult> GetBusinesses()
    {
        var businesses = await _dbContext.Businesses
            .Where(x => x.IsActive) // Filtramos a nivel de consulta SQL (Soft Delete check).
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new BusinessResponse // Proyección Select: SQL solo consulta y devuelve las columnas mapeadas del DTO.
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

        // Ok (HTTP 200 OK): Retorna la respuesta de lectura con los datos serializados en formato JSON.
        return Ok(businesses);
    }

    // GET /api/businesses/{id} (Obtener Negocio por ID):
    // Endpoint REST para recuperar los detalles de un comercio específico mediante su identificador UUID.
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
            // NotFound (HTTP 404): Informa al cliente que el recurso solicitado no existe.
            return NotFound(new
            {
                message = "Negocio no encontrado."
            });
        }

        return Ok(business);
    }

    // PATCH /api/businesses/{id}/deactivate (Inactivación Lógica de Negocio):
    // Endpoint para realizar soft delete de un comercio local.
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateBusiness(Guid id)
    {
        // FirstOrDefaultAsync: Traduce a un SELECT ... LIMIT 1 en MariaDB.
        // Busca y trae el primer objeto que coincida asíncronamente o un valor por defecto (null).
        var business = await _dbContext.Businesses
            .FirstOrDefaultAsync(x => x.Id == id);

        if (business is null)
        {
            return NotFound(new
            {
                message = "Negocio no encontrado."
            });
        }

        // Validación de Idempotencia: Verificamos el estado actual.
        if (!business.IsActive)
        {
            return BadRequest(new
            {
                message = "El negocio ya está desactivado."
            });
        }

        // SOFT DELETE (IsActive = false):
        // En lugar de hacer un delete físico destructivo en SQL ('DELETE FROM businesses'), 
        // cambiamos el bit lógico 'IsActive' a 'false'. Esto oculta el negocio en las búsquedas
        // pero preserva la integridad referencial histórica (las citas anteriores siguen apuntando a un ID válido).
        business.IsActive = false;

        // Persistimos el soft delete de forma asíncrona. EF Core genera un 'UPDATE businesses SET IsActive = 0 WHERE Id = ...'.
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
