using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DemeterDbContext _context;
        private readonly ILogger _logger;

        public UserRepository(DemeterDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<User>> FindAll(CancellationToken cancellationToken)
        {
            var res = await _context
                .Users
                .BuildUser(cancellationToken).Result
                .ToListAsync(cancellationToken);
            
            res.ForEach(u =>
            {
                u.PastOrders.ForEach(order =>
                {
                    order.Price = order.Items.Sum(i => i.Price);
                });
            });

            return res;
        }

        public Task<User> FindOne(string id, CancellationToken cancellationToken)
        {
            return _context
                .Users
                .BuildUser(cancellationToken).Result
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}