using CM_Project.Models.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CM_Project.Models
{
    public enum ApplicationStat { Pending, Approved, Rejected }

    public class ShopOwner
    {
        //i removed the shop type propery
        [Key]
        public int OwnerId { get; set; }

        [Required, StringLength(13, MinimumLength = 13)]
        public string IdBookNumber { get; set; }

        [Required, MaxLength(50)] public string Names { get; set; }
        [Required, MaxLength(50)] public string Surname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(200)] public string ResidentialAddress { get; set; }
        [Required, MaxLength(100)] public string AccountNumber { get; set; }
        [Required] public string BankType { get; set; }
        [Required, MaxLength(100)] public string ShopName { get; set; }

        [Required, MaxLength(200)] public string ShopAddress { get; set; }
        //public string Shoptyp/e { get; set; }

        // Navigation properties for related documents
        public virtual ICollection<BusRegCertificate> BussRegCertificates { get; set; } = new List<BusRegCertificate>();
        public virtual ICollection<OwnerIdentityBook> OwnerIdentityBooks { get; set; } = new List<OwnerIdentityBook>();


        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [NotMapped, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string UserType { get; set; }
        public ApplicationStat ApplicationStatus { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime RegisteredOn { get; set; } = DateTime.Now;
    }

}
