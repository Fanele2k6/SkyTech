using CM_Project.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyTech.Models
{

    public enum Delivery
    {
        Shop_Will_Collect,    //0
        Customer_Will_Bring  //1

    }
    public class ProposalCheckout
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CheckoutId { get; set; }

        // Linked to the proposal
        public int PropItemId { get; set; }
        [ForeignKey("PropItemId")]
        public virtual ProposeItem ProposeItem { get; set; }

        // Linked to the customer
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public Delivery SelectDelivery { get; set; }

        // ── Contact ──────────────────────────────
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        // ── Address ──────────────────────────────
        [Required]
        public string Address { get; set; }


        // ── Banking Details ───────────────────────
        [Required]
        public string BankName { get; set; }

        [Required]
        public string AccountHolderName { get; set; }

        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public string BranchCode { get; set; }

        public AccountType AccountType { get; set; }

        public DateTime SubmittedOn { get; set; } = DateTime.Now;
    }

    public enum AccountType
    {
        Cheque,
        Savings,
        Transmission
    }
}
