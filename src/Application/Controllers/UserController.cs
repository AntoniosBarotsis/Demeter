using System.Threading.Tasks;
using Infrastructure.Queries.Users;
using MediatR;
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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var res = await _mediator.Send(new GetAllUsersQuery());
            return Ok(res);
        }
    }
}