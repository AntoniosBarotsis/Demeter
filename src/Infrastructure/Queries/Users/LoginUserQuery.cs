using Domain.Models.Auth;
using MediatR;

namespace Infrastructure.Queries.Users
{
    public class LoginUserQuery: IRequest<AuthenticationResult>
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginUserQuery(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}