using Domain.Models.Auth;
using MediatR;

namespace Infrastructure.Commands
{
    public class RefreshTokenCommand: IRequest<AuthenticationResult>
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }

        public RefreshTokenCommand(string token, string refreshToken)
        {
            Token = token;
            RefreshToken = refreshToken;
        }
    }
}