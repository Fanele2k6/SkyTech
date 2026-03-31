using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CM_Project.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }
        public int CartId { get; set; }

        [ForeignKey("CartId")]
        public virtual Cart Carts { get; set; }

        public int ProdId { get; set; }
        public virtual AddProduct Products { get; set; }
        public string ProdName { get; set; }
        public int ProdQuantity { get; set; }
        public double PriceAtTimeAdded { get; set; }

        [NotMapped]
        public double SubTotal => ProdQuantity * PriceAtTimeAdded;
    }
}