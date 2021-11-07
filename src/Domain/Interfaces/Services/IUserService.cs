using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<User>> FindAll(CancellationToken cancellationToken);
    }
}