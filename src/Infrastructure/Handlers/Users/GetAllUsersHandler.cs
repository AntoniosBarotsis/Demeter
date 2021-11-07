using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Queries.Users;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class GetAllUsersHandler: IRequestHandler<GetAllUsersQuery, List<User>>
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsersHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return await _userRepository.FindAll(cancellationToken);
        }
    }
}