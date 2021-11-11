using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    /// <summary>
    ///     Holds data about a user order
    /// </summary>
    public class Order
    {
        public ICollection<MenuItem> Items;

        public Order()
        {
            Items = new List<MenuItem>();
            Price = 0;
        }

        [Key] public int Id { get; set; }

        // This will be calculated, not stored
        public double Price { get; set; }
    }
}