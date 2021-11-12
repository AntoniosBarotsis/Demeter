using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Services;
using Domain.Models.Auth;
using Infrastructure.Queries.Users;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class LoginUserHandler: IRequestHandler<LoginUserQuery, AuthenticationResult>
    {
        private readonly IAuthService _authService;

        public LoginUserHandler(IAuthService authService)
        {
            _authService = authService;
        }
        
        public Task<AuthenticationResult> Handle(LoginUserQuery request, CancellationToken cancellationToken)
        {
            return _authService.LoginAsync(request.Email, request.Password);
        }
    }
}