using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class MissingExtensions
    {
        public static async Task<IQueryable<User>> BuildUser(this IQueryable<User> query, CancellationToken cancellationToken)
        {
            var tmp = query
                .Include(u => u.PastOrders)
                .ThenInclude(o => o.Items)
                .AsSplitQuery()
                .OrderBy(u => u.Id);
            
            await tmp.ForEachAsync(u =>
            {
                u.PastOrders.ForEach(order =>
                {
                    order.Price = order.Items.Sum(i => i.Price);
                });
            }, cancellationToken);

            return tmp;
        }
    }
}