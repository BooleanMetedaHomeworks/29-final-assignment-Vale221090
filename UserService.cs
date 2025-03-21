public async Task<IUser> AuthenticateAsync(string email, string password)
{
    var user = GetByEmail(email);
    if (user != null && ValidatePassword(user, password))
    {
        return user;
    }
    return null;
}

private bool ValidatePassword(IUser user, string password)
{
    // Implement password validation logic here
    return true;
}
using Microsoft.IdentityModel.Tokens;
using ristorante_backend.Code;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Umbraco.Core.Services.Implement;

namespace PrimaWebApi.Services
{
    public class JwtAuthenticationService
    {
        private readonly IConfiguration _config;
        private readonly UserService _userService;

        public readonly JwtSettings Settings;

        public JwtAuthenticationService(IConfiguration config, UserService userService)
        {
            this._config = config;
            this.Settings = config.GetSection("JwtSettings")
                                    .Get<JwtSettings>();
            _userService = userService;
        }

        public async Task<string> Authenticate(string email, string password)
        {
            // Verifichiamo che nome utente e password coincidano con le info che abbiamo in DB
            var user = await _userService.AuthenticateAsync(email, password);
            if (user == null)
            {
                return null;
            }

            // Se s�, allora generiamo il token JWT
            List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email)
        };

            // Aggiungiamo eventuali ruoli
            var roles = await _userService.GetUserRolesAsync(user.Id);
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(Settings.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Settings.DurationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey),
                                            SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
