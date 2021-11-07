using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly DemeterDbContext _context;

        public UserRepository(DemeterDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> FindAll(CancellationToken cancellationToken)
        {
            return await _context
                .Users
                .Include(u => u.PastOrders)
                .ToListAsync(cancellationToken);
        }
    }
}