using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Interfaces.Services
{
    public interface IOrderService
    {
        Task<Order> AddItemToOrder(MenuItem item, int count = 1);
        Task<Order> AddItemToOrder(Order order, MenuItem item, int count = 1);
    }
}