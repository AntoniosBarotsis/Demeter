using Domain.Models.Auth;
using MediatR;

namespace Infrastructure.Commands
{
    public class RegisterUser : IRequest<AuthenticationResult>
    {
        public RegisterUser(string username, string email, string password)
        {
            Username = username;
            Email = email;
            Password = password;
        }

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}