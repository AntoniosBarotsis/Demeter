using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Domain.Models;
using Infrastructure.DTOs.Response;
using Infrastructure.Queries.Users;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class GetAllUsersHandler: IRequestHandler<GetAllUsersQuery, List<UserResponse>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetAllUsersHandler(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<List<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            return _mapper.Map<List<UserResponse>>(await _userService.FindAll(cancellationToken));
        }
    }
}