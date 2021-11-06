using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<bool> UpdateOrder(Order order);
        Task<bool> SaveChanges();
    }
}