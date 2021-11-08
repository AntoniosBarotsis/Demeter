using System.Collections.Generic;

namespace Domain.Models
{
    /// <summary>
    ///     A normal user in the application
    /// </summary>
    public class User : UserAbstract
    {
        public List<Order> PastOrders { get; set; }

        public User(string userName, string email)
        {
            UserName = userName;
            Email = email;
            PastOrders = new List<Order>();
            UserType = UserType.User;
        }
    }
}