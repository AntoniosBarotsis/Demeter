using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Queries;
using Infrastructure.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Application.Controllers
{
    /// <summary>
    ///     The users controller
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("v1/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator, ILogger logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        ///     Get all users
        /// </summary>
        /// <returns>A list of all users</returns>
        /// <remarks>This is for testing purposes and should be removed in the future</remarks>
        [HttpGet("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var res = await _mediator.Send(new GetAllUsersQuery());
            return Ok(res);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var authRes = await HttpContext.AuthenticateAsync();

                if (authRes.Principal is null) return BadRequest();
            
                var id = authRes.Principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

                var res = await _mediator.Send(new GetUserQuery(id));

                if (res is null)
                    return BadRequest("Make sure your token is valid");
                
                return Ok(res);
            }
            catch (Exception e)
            {
                _logger.Error("Exception in GetUser {Message}, stacktrace: {Stacktrace}", e.Message, e.StackTrace);
                return Problem();
            }
        }
    }
}