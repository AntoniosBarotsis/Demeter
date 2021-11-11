using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Services;
using Domain.Models.Auth;
using Infrastructure.Commands;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class RegisterUserHandler : IRequestHandler<RegisterUser, AuthenticationResult>
    {
        private readonly IAuthService _authService;

        public RegisterUserHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<AuthenticationResult> Handle(RegisterUser request, CancellationToken cancellationToken)
        {
            return await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        }
    }
}