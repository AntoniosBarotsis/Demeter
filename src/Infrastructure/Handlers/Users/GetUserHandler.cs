using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Interfaces.Services;
using Infrastructure.DTOs.Response;
using Infrastructure.Queries;
using MediatR;

namespace Infrastructure.Handlers.Users
{
    public class GetUserHandler: IRequestHandler<GetUserQuery, UserResponse>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetUserHandler(IMapper mapper, IUserService userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<UserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            var tmp = await _userService.FindOne(request.Id, cancellationToken);

            return _mapper.Map<UserResponse>(tmp);
        }
    }
}