using Domain.Models;

namespace Domain.Interfaces
{
    public interface IOrderRepository
    {
        bool UpdateOrder(Order order);
        bool SaveChanges();
    }
}