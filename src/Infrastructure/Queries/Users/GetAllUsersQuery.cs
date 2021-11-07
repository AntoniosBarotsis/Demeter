using System.Collections.Generic;
using Domain.Models;
using MediatR;

namespace Infrastructure.Queries.Users
{
    public class GetAllUsersQuery : IRequest<List<User>>
    {
        
    }
}