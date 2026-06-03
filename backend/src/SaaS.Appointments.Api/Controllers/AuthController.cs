using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Contracts.Auth;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AuthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Normalizamos los datos para evitar guardar espacios innecesarios
        // o emails con mayúsculas que luego dificulten el login.
        var fullName = request.FullName.Trim();
        var email = request.Email.Trim().ToLower();
        var password = request.Password;

        // Validación básica: el nombre es obligatorio.
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new
            {
                message = "El nombre completo es obligatorio."
            });
        }

        // Validación básica: el email es obligatorio.
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "El email es obligatorio."
            });
        }

        // Validación simple para evitar contraseñas demasiado cortas.
        // Más adelante podemos exigir mayúsculas, números o símbolos.
        if (password.Length < 6)
        {
            return BadRequest(new
            {
                message = "La contraseña debe tener al menos 6 caracteres."
            });
        }

        // Verificamos si ya existe una cuenta con ese email.
        // Esto evita depender únicamente del índice único de MariaDB.
        var emailAlreadyExists = await _dbContext.Users
            .AnyAsync(x => x.Email == email);

        if (emailAlreadyExists)
        {
            return Conflict(new
            {
                message = "Ya existe un usuario con ese email."
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,

            // El backend asigna el rol.
            // El frontend no puede decidir si alguien será Admin.
            Role = UserRole.Staff,

            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        // PasswordHasher genera un hash seguro a partir de la contraseña.
        // Nunca guardamos la contraseña original.
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        _dbContext.Users.Add(user);

        // Aquí EF Core ejecuta el INSERT real en MariaDB.
        await _dbContext.SaveChangesAsync();

        var response = new AuthUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAtUtc = user.CreatedAtUtc
        };

        return CreatedAtAction(
            nameof(Register),
            new { id = user.Id },
            response
        );
    }
}
