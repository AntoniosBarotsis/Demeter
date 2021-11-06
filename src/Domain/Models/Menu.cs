using System.Collections.Generic;

namespace Domain.Models
{
    /// <summary>
    ///     Holds all the meal options from a given restaurant
    /// </summary>
    public class Menu
    {
        public Menu()
        {
            MenuItems = new List<MenuItem>();
        }

        public ICollection<MenuItem> MenuItems { get; }
    }
}