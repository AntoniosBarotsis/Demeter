using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.Queries.Users;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class GetAllUsersHandler: IRequestHandler<GetAllUsersQuery, List<User>>
    {
        private readonly IUserService _userService;

        public GetAllUsersHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userService.FindAll(cancellationToken);
        }
    }
}