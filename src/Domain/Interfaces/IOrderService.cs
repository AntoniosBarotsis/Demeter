using Domain.Models;

namespace Domain.Interfaces
{
    public interface IOrderService
    {
        public Order AddItemToOrder(MenuItem item, int count = 1);
        public Order AddItemToOrder(Order order, MenuItem item, int count = 1);
    }
}