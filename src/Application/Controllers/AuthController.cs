using System.Linq;
using System.Threading.Tasks;
using Domain.Models.Auth;
using Infrastructure.Commands;
using Infrastructure.DTOs.Request;
using Infrastructure.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public AuthController(ILogger logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        ///     Registers a user to the database. Returns a token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                   ErrorMessages = ModelState.Values
                       .SelectMany(v => v.Errors.Select(err => err.ErrorMessage))
                });
            }
            
            var authResponse =
                await _mediator.Send(new RegisterUserCommand(request.UserName, request.Email, request.Password));

            return HandleAuthenticationResult(authResponse);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var authResponse = 
                await _mediator.Send(new LoginUserQuery(request.Email, request.Password));

            return HandleAuthenticationResult(authResponse);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var authResponse = 
                await _mediator.Send(new RefreshTokenCommand(request.Token, request.RefreshToken));
            
            return HandleAuthenticationResult(authResponse);
        }
        
        private IActionResult HandleAuthenticationResult(AuthenticationResult authResponse)
        {
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    ErrorMessages = authResponse.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }
    }
}