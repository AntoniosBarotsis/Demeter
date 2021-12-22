using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    /// <summary>
    ///     Holds data about a user order
    /// </summary>
    public class Order
    {
        [Key] public int Id { get; set; }
        public ICollection<MenuItem> Items { get; set; }
        [NotMapped] public double Price { get; set; }

        public Order()
        {
            Items = new List<MenuItem>();
        }
    }
}