using System.ComponentModel.DataAnnotations;

namespace ristorante_backend.Models
{
    public class UserModel
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
