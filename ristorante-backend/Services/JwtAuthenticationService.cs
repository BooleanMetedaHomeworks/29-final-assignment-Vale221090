using Microsoft.IdentityModel.Tokens;
using ristorante_backend.Code;
using ristorante_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ristorante_backend.Services
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
    }

    public class JwtAuthenticationService
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        public required JwtSettings Settings { get; init; }

        public JwtAuthenticationService(IConfiguration config, IUserService userService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            var settings = config.GetSection("JwtSettings").Get<JwtSettings>();
            if (settings == null)
                throw new InvalidOperationException("JwtSettings configuration is missing");

            Settings = settings;
        }

        public async Task<string?> Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                return null;

            // Autenticazione utente
            var user = await _userService.AuthenticateAsync(email, password);
            if (user == null)
                return null;

            // Generazione claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Aggiunta ruoli
            var roles = await _userService.GetUserRolesAsync(user.Id);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Generazione token
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(Settings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Settings.DurationInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = "ristorante-backend",
                Audience = "ristorante-frontend"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class JwtSettings
    {
        public required string Key { get; set; }
        public int DurationInMinutes { get; set; } = 60; // Default 1 ora
    }

    // Modello base per l'utente
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}
