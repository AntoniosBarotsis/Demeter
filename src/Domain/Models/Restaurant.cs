using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace Domain.Models
{
    public class Restaurant
    {
        [Key] public long Id { get; set; }

        [Required] public string Name { get; set; }

        [Required] public Point Location { get; set; }

        public Menu Menu { get; set; }
    }
}