using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaS.Appointments.Api.Auth;
using SaaS.Appointments.Api.Contracts.Auth;
using SaaS.Appointments.Domain.Entities;
using SaaS.Appointments.Infrastructure.Persistence;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SaaS.Appointments.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        AppDbContext dbContext,
        JwtTokenService jwtTokenService)
    {
        // AppDbContext permite consultar usuarios en MariaDB.
        _dbContext = dbContext;

        // JwtTokenService genera el token cuando el login es correcto.
        _jwtTokenService = jwtTokenService;
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

        // Revisamos si ya existe al menos un usuario en la tabla users.
        // Esta regla facilita el uso open source del proyecto:
        // - si no hay usuarios, el primero será Admin;
        // - si ya hay usuarios, los siguientes serán Staff.
        var hasAnyUser = await _dbContext.Users.AnyAsync();

        var assignedRole = hasAnyUser
            ? UserRole.Staff
            : UserRole.Admin;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,

            // El frontend no decide el rol.
            // El backend lo asigna de forma controlada.
            Role = assignedRole,

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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Normalizamos el email para que el login no falle por mayúsculas o espacios.
        var email = request.Email.Trim().ToLower();
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "El email es obligatorio."
            });
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return BadRequest(new
            {
                message = "La contraseña es obligatoria."
            });
        }

        // Buscamos el usuario por email.
        // Como Email tiene índice único, debería existir máximo un usuario con ese correo.
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        // Por seguridad usamos un mensaje genérico.
        // No decimos si falló el email o la contraseña para no dar pistas.
        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales inválidas."
            });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new
            {
                message = "La cuenta está desactivada."
            });
        }

        // PasswordHasher compara la contraseña escrita con el hash guardado.
        // No desencripta el hash; solo verifica si coinciden.
        var passwordHasher = new PasswordHasher<User>();

        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
        );

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Credenciales inválidas."
            });
        }

        // Si llegamos aquí, el email existe, la cuenta está activa
        // y la contraseña coincide con el hash guardado.
        var token = _jwtTokenService.GenerateToken(user);

        var response = new LoginResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,

            // Enviamos el JWT al frontend para que pueda autenticarse
            // en futuras peticiones protegidas.
            Token = token
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        // Cuando el frontend manda:
        // Authorization: Bearer {token}
        // ASP.NET Core valida el JWT y llena la propiedad User con los claims del token.

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        // Si el token es válido, estos datos deberían existir.
        // Si no existieran, significa que el token no tiene los claims esperados.
        if (userId is null || email is null || fullName is null || role is null)
        {
            return Unauthorized(new
            {
                message = "Token inválido o incompleto."
            });
        }

        return Ok(new
        {
            id = userId,
            fullName,
            email,
            role
        });
    }
}
