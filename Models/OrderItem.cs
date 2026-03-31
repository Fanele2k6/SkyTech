using System.ComponentModel.DataAnnotations;

namespace CM_Project.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public int ProdId { get; set; }
        public virtual AddProduct Products { get; set; }
        public string ProdName { get; set; }
        public int Quantity { get; set; }
        public double PriceAtTimeOfPurchase { get; set; }
    }
}