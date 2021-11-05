using System;
using System.Collections.Generic;

namespace Domain.Models
{
    /// <summary>
    ///     Holds data about a user order
    /// </summary>
    public class Order
    {
        private Guid _id;
        public ICollection<MenuItem> Items;

        public Order()
        {
            _id = new Guid();
            Items = new List<MenuItem>();
            Price = 0;
        }

        // This will be calculated, not stored
        public double Price { get; set; }
    }
}