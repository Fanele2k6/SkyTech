using System.ComponentModel.DataAnnotations;

namespace SkyTech.Models
{

    public enum ChooseDelivery
    {
        Store_Will_Collect,
        Customer_Will_Bring
    }
    public class AcceptOffer
    {
        // Notify them to bring their ID
        [Key]
        public int OfferId { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        //The existing Email will be fetched  
        public string Address { get; set; } // only required if user requested collection

        [Required]
        public string BankName { get; set; }

        [Required]
        public string AccountNumber { get; set; }





    }
}
