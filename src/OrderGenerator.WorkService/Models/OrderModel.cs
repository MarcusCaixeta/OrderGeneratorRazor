using System.ComponentModel.DataAnnotations;

namespace OrderGenerator.WorkerService.Models
{
    public class OrderModel
    {
        [Required]
        [RegularExpression("PETR4|VALE3|VIIA4")]
        public string? Symbol { get; set; }

        [Required]
        [RegularExpression("Compra|Venda")]
        public string? Side { get; set; }

        [Required]
        [Range(1, 99999)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 999.99)]
        public decimal Price { get; set; }
    }
}
