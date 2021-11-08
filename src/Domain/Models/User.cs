using System;
using System.Collections.Generic;
using System.Linq;
using KellermanSoftware.CompareNetObjects;

namespace Domain.Models
{
    /// <summary>
    ///     A normal user in the application
    /// </summary>
    public class User : UserAbstract
    {
        private readonly CompareLogic _compareLogic = new();

        public User(string userName, string email)
        {
            UserName = userName;
            Email = email;
            PastOrders = new List<Order>();
            UserType = UserType.User;
        }

        public List<Order> PastOrders { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && _compareLogic.Compare(this, obj).AreEqual;
        }

        private bool Equals(User other)
        {
            return _compareLogic.Compare(this, other).AreEqual && PastOrders.SequenceEqual(other.PastOrders);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_compareLogic, PastOrders);
        }

        public static bool operator ==(User left, User right)
        {
            return left is not null && left.Equals(right);
        }

        public static bool operator !=(User left, User right)
        {
            return left is not null && !left.Equals(right);
        }
    }
}