using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();

        if (!canConnect)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "No se pudo conectar a MariaDB"
            });
        }

        return Ok(new
        {
            status = "ok",
            message = "Conexión a MariaDB correcta"
        });
    }
}
