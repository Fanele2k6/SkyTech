using CM_Project.Models.Documents;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CM_Project.Models
{
    public enum SelectCategory
    {
        Local_Favorites, Electronics, Baby_and_Kids,
        Personal_Care, Fresh_Produce, Drinks,
        Snacks, Groceries, Stationary
    }

    public enum ProdStatus
    {
        In_Stock,    // 0
        Low_stock,   // 1
        Out_Of_Stock // 2
    }

    public class AddProduct
    {
        [Key]
        public int ProdId { get; set; }
        public int OwnerId { get; set; }
        public virtual ShopOwner Owner { get; set; }

        [Required] public string ProdName { get; set; }
        [Required] public SelectCategory ProdCategory { get; set; }
        [Required] public string ProdDescription { get; set; }
        [Required] public double ProdPrice { get; set; }
        [Required] public string ProdSize { get; set; }
        [Required] public int ProdQuantity { get; set; }

        public virtual ICollection<ProductPicture> ProductPictures { get; set; } = new List<ProductPicture>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public ProdStatus Status { get; set; }

        [NotMapped]
        public string ShopName { get; set; }
    }
}

