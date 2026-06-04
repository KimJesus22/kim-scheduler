using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaaS.Appointments.Domain.Entities;

namespace SaaS.Appointments.Api.Auth;

// Servicio encargado de generar tokens JWT.
// Lo separamos para no meter toda la lógica de tokens dentro del AuthController.
public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(User user)
    {
        // Claims = datos que viajan dentro del token.
        // No metas información sensible aquí, porque el JWT puede ser leído por el cliente.
        var claims = new List<Claim>
        {
            // Identificador único del usuario.
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

            // Email del usuario autenticado.
            new Claim(ClaimTypes.Email, user.Email),

            // Nombre visible del usuario.
            new Claim(ClaimTypes.Name, user.FullName),

            // Rol usado después por [Authorize(Roles = "...")].
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Convertimos el SecretKey en bytes para firmar el token.
        // La firma permite que el backend detecte si alguien alteró el JWT.
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)
        );

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(
            _jwtSettings.ExpirationMinutes
        );

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
