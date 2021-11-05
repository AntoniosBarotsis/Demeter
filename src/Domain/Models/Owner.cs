using System.Collections.Generic;

namespace Domain.Models
{
    /// <summary>
    ///     A (restaurant) owner user.
    /// </summary>
    public class Owner : UserAbstract
    {
        public ICollection<Restaurant> Restaurants { get; set; }
    }
}