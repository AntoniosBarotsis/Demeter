using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    /// <summary>
    ///     Represents any item you could order from a restaurant
    /// </summary>
    public class MenuItem
    {
        public MenuItem(string name, string description, double price, string image)
        {
            Name = name;
            Description = description;
            Price = price;
            Image = image;
            IsAvailable = true;
        }

        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public bool IsAvailable { get; set; }
    }
}