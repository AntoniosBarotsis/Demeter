using System.ComponentModel.DataAnnotations;

namespace Infrastructure.DTOs.Request
{
    public class UserRegistrationRequest
    {
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}