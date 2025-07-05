
using System.ComponentModel.DataAnnotations;

namespace OrderGenerator.Models
{
    public class OrderModel
    {
        [Required]
        public string? Symbol { get; set; }

        [Required]
        public string? Side { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
