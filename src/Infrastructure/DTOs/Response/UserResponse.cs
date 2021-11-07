using System.Collections.Generic;
using Domain.Models;

namespace Infrastructure.DTOs.Response
{
    public class UserResponse
    {
        public string UserName { get; set; }
        public List<Order> PastOrders { get; set; }
    }
}