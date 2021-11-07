using System.Threading.Tasks;
using Infrastructure.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController: ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator, ILogger logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var res = await _mediator.Send(new GetAllUsersQuery());
            return Ok(res);
        }
    }
}