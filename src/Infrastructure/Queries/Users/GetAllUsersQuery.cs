using System.Collections.Generic;
using Infrastructure.DTOs.Response;
using MediatR;

namespace Infrastructure.Queries.Users
{
    public class GetAllUsersQuery : IRequest<List<UserResponse>>
    {
    }
}