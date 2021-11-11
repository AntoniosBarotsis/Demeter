using System.Threading.Tasks;
using Domain.Models.Auth;
using Infrastructure.Commands;
using Infrastructure.DTOs.Request;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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
            var authResponse =
                await _mediator.Send(new RegisterUser(request.UserName, request.Email, request.Password));

            if (!authResponse.Success)
                return BadRequest(new AuthFailedResponse
                {
                    ErrorMessages = authResponse.Errors
                });

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token
            });
        }
    }
}