using System;
using Domain.Interfaces;
using Domain.Models;

namespace Domain.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Order AddItemToOrder(MenuItem item, int count = 1)
        {
            return AddItemToOrder(new Order(), item, count);
        }

        public Order AddItemToOrder(Order order, MenuItem item, int count = 1)
        {
            if (count <= 0) throw new ArgumentException($"count cannot be less than 0. Was {count}");

            for (var i = 0; i < count; i++) order.Items.Add(item);

            order.Price += item.Price * count;

            if (!_orderRepository.UpdateOrder(order)) throw new Exception("Something went wrong");

            return order;
        }
    }
}