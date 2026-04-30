using CM_Project.Models;
using CM_Project.Models.Documents;
using System.ComponentModel.DataAnnotations;

namespace SkyTech.Models
{
    public enum SelectCondition
    {
        New,         //0
        Used_like_New, //1
        Used,  //2
        Broken //3
    }
    public class ProposeItem
    {
        [Key]
        public int PropItemId { get; set; }

        [Required]
        public string ProdName { get; set; }

        [Required]
        public SelectCategory ProdCategory { get; set; }

        [Required]
        public SelectCondition ProdCondition { get; set; }

        [Required]
        public string ProdDescription { get; set; }

        public virtual ICollection<ReceiptPic> ReceiptPics { get; set; } = new List<ReceiptPic>();
        public virtual ICollection<ProposedProdPic> ProposedProdPics { get; set; } = new List<ProposedProdPic>();

        //Proposed product pictures

        // You must add serial number in pictures that are uploaded

        public Status ProdStatus { get; set; }

        public DateTime SavedOn { get; set; }
    }
}
