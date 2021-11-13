using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Services;
using Domain.Models.Auth;
using Infrastructure.Commands;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class RefreshTokenHandler: IRequestHandler<RefreshTokenCommand, AuthenticationResult>
    {
        private readonly IAuthService _authService;

        public RefreshTokenHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public Task<AuthenticationResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
        }
    }
}