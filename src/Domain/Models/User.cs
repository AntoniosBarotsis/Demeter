using System.Collections.Generic;

namespace Domain.Models
{
    /// <summary>
    ///     A normal user in the application
    /// </summary>
    public class User : UserAbstract
    {
        private ICollection<Order> PastOrders { get; set; }
    }
}